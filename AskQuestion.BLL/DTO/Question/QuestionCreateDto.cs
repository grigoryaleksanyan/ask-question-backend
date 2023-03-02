using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskQuestion.BLL.DTO.Question
{
    public class QuestionCreateDto
    {
        public string? Author { get; set; }

        public string Speaker { get; set; } = string.Empty;

        public string? Zone { get; set; }

        public string Text { get; set; } = string.Empty;
    }
}
