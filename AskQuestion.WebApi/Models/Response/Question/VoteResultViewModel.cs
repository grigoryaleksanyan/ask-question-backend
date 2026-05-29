using AskQuestion.Core.Enums;
using System.Text.Json.Serialization;

namespace AskQuestion.WebApi.Models.Response.Question
{
    public class VoteResultViewModel
    {
        public int Likes { get; set; }
        public int Dislikes { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VoteType? UserVote { get; set; }
    }
}
