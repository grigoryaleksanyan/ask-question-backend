using AskQuestion.Core.Enums;

namespace AskQuestion.BLL.DTO.Question
{
    public class VoteResultDto
    {
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public VoteType? UserVote { get; set; }
    }
}
