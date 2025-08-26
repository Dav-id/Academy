namespace Academy.Services.Api.Extensions
{
    public static class AssessmentQuestionTypeExtensions
    {
        /// <summary>
        /// Converts a <see cref="Academy.Services.Api.Endpoints.Assessments.AssessmentQuestionType"/> to an <see cref="Academy.Shared.Data.Models.Assessments.Enums.AssessmentQuestionType"/> enum value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The corresponding <see cref="Academy.Services.Api.Endpoints.Assessments.AssessmentQuestionType"/> enum value.</returns>
        public static Academy.Shared.Data.Models.Assessments.Enums.AssessmentQuestionType ConvertAssessmentQuestionType(this Academy.Services.Api.Endpoints.Assessments.AssessmentQuestionType value)
        {
            return value switch
            {
                Academy.Services.Api.Endpoints.Assessments.AssessmentQuestionType.SingleChoice => Academy.Shared.Data.Models.Assessments.Enums.AssessmentQuestionType.SingleChoice,
                Academy.Services.Api.Endpoints.Assessments.AssessmentQuestionType.MultipleChoice => Academy.Shared.Data.Models.Assessments.Enums.AssessmentQuestionType.MultipleChoice,
                Academy.Services.Api.Endpoints.Assessments.AssessmentQuestionType.Boolean => Academy.Shared.Data.Models.Assessments.Enums.AssessmentQuestionType.Boolean,
                Academy.Services.Api.Endpoints.Assessments.AssessmentQuestionType.ShortAnswer => Academy.Shared.Data.Models.Assessments.Enums.AssessmentQuestionType.ShortAnswer,
                Academy.Services.Api.Endpoints.Assessments.AssessmentQuestionType.LongAnswer => Academy.Shared.Data.Models.Assessments.Enums.AssessmentQuestionType.LongAnswer,
                Academy.Services.Api.Endpoints.Assessments.AssessmentQuestionType.IntegerAnswer => Academy.Shared.Data.Models.Assessments.Enums.AssessmentQuestionType.IntegerAnswer,
                Academy.Services.Api.Endpoints.Assessments.AssessmentQuestionType.DecimalAnswer => Academy.Shared.Data.Models.Assessments.Enums.AssessmentQuestionType.DecimalAnswer,

                _ => throw new ArgumentException($"Invalid type: {value}")
            };
        }

        /// <summary>
        /// Converts a <see cref="Academy.Shared.Data.Models.Assessments.Enums.AssessmentQuestionType"/> to an <see cref="Academy.Services.Api.Endpoints.Assessments.AssessmentQuestionType"/> enum value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The corresponding <see cref="Academy.Shared.Data.Models.Assessments.Enums.AssessmentQuestionType"/> enum value.</returns>
        public static Academy.Services.Api.Endpoints.Assessments.AssessmentQuestionType ConvertAssessmentQuestionType(this Academy.Shared.Data.Models.Assessments.Enums.AssessmentQuestionType value)
        {
            return value switch
            {
                Academy.Shared.Data.Models.Assessments.Enums.AssessmentQuestionType.SingleChoice => Academy.Services.Api.Endpoints.Assessments.AssessmentQuestionType.SingleChoice,
                Academy.Shared.Data.Models.Assessments.Enums.AssessmentQuestionType.MultipleChoice => Academy.Services.Api.Endpoints.Assessments.AssessmentQuestionType.MultipleChoice,
                Academy.Shared.Data.Models.Assessments.Enums.AssessmentQuestionType.Boolean => Academy.Services.Api.Endpoints.Assessments.AssessmentQuestionType.Boolean,
                Academy.Shared.Data.Models.Assessments.Enums.AssessmentQuestionType.ShortAnswer => Academy.Services.Api.Endpoints.Assessments.AssessmentQuestionType.ShortAnswer,
                Academy.Shared.Data.Models.Assessments.Enums.AssessmentQuestionType.LongAnswer => Academy.Services.Api.Endpoints.Assessments.AssessmentQuestionType.LongAnswer,
                Academy.Shared.Data.Models.Assessments.Enums.AssessmentQuestionType.IntegerAnswer => Academy.Services.Api.Endpoints.Assessments.AssessmentQuestionType.IntegerAnswer,
                Academy.Shared.Data.Models.Assessments.Enums.AssessmentQuestionType.DecimalAnswer => Academy.Services.Api.Endpoints.Assessments.AssessmentQuestionType.DecimalAnswer,
                _ => throw new ArgumentException($"Invalid type: {value}")
            };
        }
    }
}
