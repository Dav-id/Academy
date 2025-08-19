namespace Academy.Services.Api.Extensions
{
    public static class QuizQuestionTypeExtensions
    {
        /// <summary>
        /// Converts a <see cref="Academy.Services.Api.Endpoints.Assessments.QuizQuestionType"/> to an <see cref="Academy.Shared.Data.Models.Assessments.Enums.QuizQuestionType"/> enum value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The corresponding <see cref="Academy.Services.Api.Endpoints.Assessments.QuizQuestionType"/> enum value.</returns>
        public static Academy.Shared.Data.Models.Assessments.Enums.QuizQuestionType ConvertQuizQuestionType(this Academy.Services.Api.Endpoints.Assessments.QuizQuestionType value)
        {
            return value switch
            {
                Academy.Services.Api.Endpoints.Assessments.QuizQuestionType.SingleChoice => Academy.Shared.Data.Models.Assessments.Enums.QuizQuestionType.SingleChoice,
                Academy.Services.Api.Endpoints.Assessments.QuizQuestionType.MultipleChoice => Academy.Shared.Data.Models.Assessments.Enums.QuizQuestionType.MultipleChoice,
                Academy.Services.Api.Endpoints.Assessments.QuizQuestionType.TrueFalse => Academy.Shared.Data.Models.Assessments.Enums.QuizQuestionType.TrueFalse,
                Academy.Services.Api.Endpoints.Assessments.QuizQuestionType.ShortAnswer => Academy.Shared.Data.Models.Assessments.Enums.QuizQuestionType.ShortAnswer,
                Academy.Services.Api.Endpoints.Assessments.QuizQuestionType.LongAnswer => Academy.Shared.Data.Models.Assessments.Enums.QuizQuestionType.LongAnswer,
                _ => throw new ArgumentException($"Invalid type: {value}")
            };
        }

        /// <summary>
        /// Converts a <see cref="Academy.Shared.Data.Models.Assessments.Enums.QuizQuestionType"/> to an <see cref="Academy.Services.Api.Endpoints.Assessments.QuizQuestionType"/> enum value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The corresponding <see cref="Academy.Shared.Data.Models.Assessments.Enums.QuizQuestionType"/> enum value.</returns>
        public static Academy.Services.Api.Endpoints.Assessments.QuizQuestionType ConvertQuizQuestionType(this Academy.Shared.Data.Models.Assessments.Enums.QuizQuestionType value)
        {
            return value switch
            {
                Academy.Shared.Data.Models.Assessments.Enums.QuizQuestionType.SingleChoice => Academy.Services.Api.Endpoints.Assessments.QuizQuestionType.SingleChoice,
                Academy.Shared.Data.Models.Assessments.Enums.QuizQuestionType.MultipleChoice => Academy.Services.Api.Endpoints.Assessments.QuizQuestionType.MultipleChoice,
                Academy.Shared.Data.Models.Assessments.Enums.QuizQuestionType.TrueFalse => Academy.Services.Api.Endpoints.Assessments.QuizQuestionType.TrueFalse,
                Academy.Shared.Data.Models.Assessments.Enums.QuizQuestionType.ShortAnswer => Academy.Services.Api.Endpoints.Assessments.QuizQuestionType.ShortAnswer,
                Academy.Shared.Data.Models.Assessments.Enums.QuizQuestionType.LongAnswer => Academy.Services.Api.Endpoints.Assessments.QuizQuestionType.LongAnswer,
                _ => throw new ArgumentException($"Invalid type: {value}")
            };
        }
    }
}
