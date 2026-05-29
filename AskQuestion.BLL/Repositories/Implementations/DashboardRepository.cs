using AskQuestion.BLL.DTO.Dashboard;
using AskQuestion.BLL.Repositories.Interfaces;
using AskQuestion.Core.Enums;
using AskQuestion.DAL;
using Microsoft.EntityFrameworkCore;

namespace AskQuestion.BLL.Repositories.Implementations;

public class DashboardRepository(DataContext dataContext) : IDashboardRepository
{
    public async Task<DashboardSummaryDto> GetSummaryAsync(int periodDays)
    {
        var now = DateTimeOffset.UtcNow;
        var periodStart = now.AddDays(-periodDays);

        var totalQuestions = await dataContext.Questions.CountAsync();
        var answeredQuestions = await dataContext.Questions
            .CountAsync(q => q.Status == (int)QuestionStatus.Answered);
        var totalFeedback = await dataContext.Feedback.CountAsync();
        var totalAreas = await dataContext.Areas.CountAsync();

        var byStatus = await dataContext.Questions
            .AsNoTracking()
            .GroupBy(q => q.Status)
            .Select(g => new StatusDistributionDto
            {
                Status = g.Key,
                Count = g.Count(),
            })
            .ToListAsync();

        var createdDates = await dataContext.Questions
            .AsNoTracking()
            .Where(q => q.Created >= periodStart)
            .Select(q => q.Created)
            .ToListAsync();

        var answeredDates = await dataContext.Questions
            .AsNoTracking()
            .Where(q => q.Answered.HasValue && q.Answered.Value >= periodStart)
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

        var byArea = await dataContext.Questions
            .AsNoTracking()
            .Where(q => q.Area != null && q.Area != string.Empty)
            .GroupBy(q => q.Area!)
            .Select(g => new AreaDistributionDto
            {
                AreaTitle = g.Key,
                Count = g.Count(),
            })
            .OrderByDescending(g => g.Count)
            .ToListAsync();

        var topSpeakers = await dataContext.Questions
            .AsNoTracking()
            .Where(q => q.Status == (int)QuestionStatus.Answered
                     && q.Speaker != string.Empty)
            .GroupBy(q => q.Speaker)
            .Select(g => new SpeakerStatsDto
            {
                SpeakerName = g.Key,
                AnsweredCount = g.Count(),
            })
            .OrderByDescending(g => g.AnsweredCount)
            .Take(5)
            .ToListAsync();

        var totalLikes = await dataContext.QuestionVotes
            .CountAsync(v => v.VoteType == VoteType.Like);
        var totalDislikes = await dataContext.QuestionVotes
            .CountAsync(v => v.VoteType == VoteType.Dislike);

        return new DashboardSummaryDto
        {
            TotalQuestions = totalQuestions,
            AnsweredQuestions = answeredQuestions,
            TotalFeedback = totalFeedback,
            TotalAreas = totalAreas,
            ByStatus = byStatus,
            Timeline = timeline,
            ByArea = byArea,
            TopSpeakers = topSpeakers,
            Votes = new VotesSummaryDto
            {
                TotalLikes = totalLikes,
                TotalDislikes = totalDislikes,
            },
        };
    }
}
