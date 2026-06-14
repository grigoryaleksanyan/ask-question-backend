using AskQuestion.BLL.DTO.Dashboard;
using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.Core.Enums;
using AskQuestion.DAL;
using Microsoft.EntityFrameworkCore;

namespace AskQuestion.BLL.Repositories.Implementations;

public class DashboardRepository(DataContext dataContext) : IDashboardRepository
{
    public async Task<DashboardSummaryDto> GetSummaryAsync(int periodDays, Guid? speakerId = null)
    {
        var now = DateTimeOffset.UtcNow;
        var periodStart = now.AddDays(-periodDays);

        var baseQuery = dataContext.Questions
            .AsNoTracking()
            .Where(q => q.Created >= periodStart);

        if (speakerId.HasValue)
        {
            baseQuery = baseQuery.Where(q => q.SpeakerId == speakerId.Value);
        }

        var totalQuestions = await baseQuery.CountAsync();
        var answeredQuestions = await baseQuery
            .CountAsync(q => q.Status == (int)QuestionStatus.Answered);
        var unansweredQuestions = totalQuestions - answeredQuestions;

        var answeredPairs = await baseQuery
            .Where(q => q.Answered.HasValue)
            .Select(q => new { q.Created, Answered = q.Answered!.Value })
            .ToListAsync();

        var avgResponseHours = answeredPairs.Count > 0
            ? answeredPairs.Average(p => (p.Answered - p.Created).TotalHours)
            : 0;

        var totalFeedback = await dataContext.Feedback
            .CountAsync(f => f.Created >= periodStart);
        var totalAreas = await dataContext.Areas.CountAsync();
        var questionsWithoutSpeaker = await baseQuery
            .CountAsync(q => !q.SpeakerId.HasValue);

        var byStatus = await baseQuery
            .GroupBy(q => q.Status)
            .Select(g => new StatusDistributionDto
            {
                Status = g.Key,
                Count = g.Count(),
            })
            .ToListAsync();

        var createdDates = await baseQuery
            .Select(q => q.Created)
            .ToListAsync();

        var answeredDates = await baseQuery
            .Where(q => q.Answered.HasValue)
            .Select(q => q.Answered!.Value)
            .ToListAsync();

        var timeline = new List<TimelinePointDto>();
        for (var i = periodDays - 1; i >= 0; i--)
        {
            var date = now.AddDays(-i).Date;
            var dateStr = date.ToString("yyyy-MM-dd");
            var newCount = createdDates.Count(d => d.Date == date);
            var answeredCount = answeredDates.Count(d => d.Date == date);
            timeline.Add(new TimelinePointDto
            {
                Date = dateStr,
                NewCount = newCount,
                AnsweredCount = answeredCount,
            });
        }

        var byArea = await baseQuery
            .Include(q => q.AreaEntity)
            .Where(q => q.AreaId.HasValue)
            .GroupBy(q => q.AreaEntity!.Title)
            .Select(g => new AreaDistributionDto
            {
                AreaTitle = g.Key,
                Count = g.Count(),
            })
            .OrderByDescending(g => g.Count)
            .ToListAsync();

        var questionIds = await baseQuery.Select(q => q.Id).ToListAsync();
        var totalLikes = await dataContext.QuestionVotes
            .Where(v => questionIds.Contains(v.QuestionId) && v.VoteType == VoteType.Like)
            .CountAsync();
        var totalDislikes = await dataContext.QuestionVotes
            .Where(v => questionIds.Contains(v.QuestionId) && v.VoteType == VoteType.Dislike)
            .CountAsync();

        var topSpeakers = await BuildTopSpeakersAsync(baseQuery, speakerId);
        var speakerAreas = await BuildSpeakerAreasAsync(baseQuery);

        return new DashboardSummaryDto
        {
            TotalQuestions = totalQuestions,
            AnsweredQuestions = answeredQuestions,
            UnansweredQuestions = unansweredQuestions,
            AverageResponseTimeHours = Math.Round(avgResponseHours, 1),
            TotalFeedback = totalFeedback,
            TotalAreas = totalAreas,
            QuestionsWithoutSpeaker = questionsWithoutSpeaker,
            ByStatus = byStatus,
            Timeline = timeline,
            ByArea = byArea,
            TopSpeakers = topSpeakers,
            SpeakerAreas = speakerAreas,
            Votes = new VotesSummaryDto
            {
                TotalLikes = totalLikes,
                TotalDislikes = totalDislikes,
            },
        };
    }

    private async Task<List<SpeakerProductivityDto>> BuildTopSpeakersAsync(
        IQueryable<DAL.Entities.Question> baseQuery, Guid? speakerId)
    {
        var takeCount = speakerId.HasValue ? 1 : 5;

        var speakerAggregates = await baseQuery
            .Where(q => q.SpeakerId.HasValue)
            .GroupBy(q => q.SpeakerId!.Value)
            .Select(g => new
            {
                SpeakerId = g.Key,
                Assigned = g.Count(),
                Answered = g.Count(q => q.Status == (int)QuestionStatus.Answered),
            })
            .OrderByDescending(g => g.Answered)
            .Take(takeCount)
            .ToListAsync();

        var speakerIds = speakerAggregates.Select(g => g.SpeakerId).ToList();

        var speakerAnsweredDates = await baseQuery
            .Where(q => q.SpeakerId.HasValue && speakerIds.Contains(q.SpeakerId!.Value) && q.Answered.HasValue)
            .Select(q => new { SpeakerId = q.SpeakerId!.Value, q.Created, Answered = q.Answered!.Value })
            .ToListAsync();

        var avgHoursBySpeaker = speakerAnsweredDates
            .GroupBy(x => x.SpeakerId)
            .ToDictionary(
                g => g.Key,
                g => g.Average(p => (p.Answered - p.Created).TotalHours));

        var speakerNames = await dataContext.Users
            .AsNoTracking()
            .Include(u => u.UserDetails)
            .Where(u => speakerIds.Contains(u.Id) && u.UserDetails != null && u.IsActive)
            .ToDictionaryAsync(
                u => u.Id,
                u => u.UserDetails!.GetFullName());

        return speakerAggregates.Select(g => new SpeakerProductivityDto
        {
            SpeakerId = g.SpeakerId,
            SpeakerName = speakerNames.GetValueOrDefault(g.SpeakerId, "Unknown"),
            AssignedQuestions = g.Assigned,
            AnsweredQuestions = g.Answered,
            AnswerRate = g.Assigned > 0 ? Math.Round((double)g.Answered / g.Assigned * 100, 1) : 0,
            AverageResponseHours = Math.Round(avgHoursBySpeaker.GetValueOrDefault(g.SpeakerId, 0), 1),
        }).ToList();
    }

    private async Task<List<SpeakerAreaDto>> BuildSpeakerAreasAsync(
        IQueryable<DAL.Entities.Question> baseQuery)
    {
        var rawData = await baseQuery
            .Where(q => q.SpeakerId.HasValue && q.AreaId.HasValue)
            .Select(q => new { q.SpeakerId, AreaTitle = q.AreaEntity != null ? q.AreaEntity.Title : "" })
            .ToListAsync();

        var grouped = rawData
            .GroupBy(x => new { SpeakerId = x.SpeakerId!.Value, x.AreaTitle })
            .Select(g => new SpeakerAreaDto
            {
                SpeakerId = g.Key.SpeakerId,
                AreaTitle = g.Key.AreaTitle,
                QuestionCount = g.Count(),
            })
            .ToList();

        var speakerIds = grouped.Select(g => g.SpeakerId).Distinct().ToList();
        var speakerNames = await dataContext.Users
            .AsNoTracking()
            .Include(u => u.UserDetails)
            .Where(u => speakerIds.Contains(u.Id) && u.UserDetails != null && u.IsActive)
            .ToDictionaryAsync(
                u => u.Id,
                u => u.UserDetails!.GetFullName());

        foreach (var item in grouped)
        {
            item.SpeakerName = speakerNames.GetValueOrDefault(item.SpeakerId, "Unknown");
        }

        return grouped;
    }
}
