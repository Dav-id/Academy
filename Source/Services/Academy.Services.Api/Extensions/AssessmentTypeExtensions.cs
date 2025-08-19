namespace Academy.Services.Api.Extensions
{
    public static class AssessmentTypeExtensions
    {
        /// <summary>
        /// Converts a string to an <see cref="Academy.Shared.Data.Models.Assessments.Enums.AssessmentType"/> enum value.
        /// </summary>
        /// <param name="value">The string value to convert.</param>
        /// <returns>The corresponding <see cref="Academy.Services.Api.Endpoints.Assessments.AssessmentType"/> enum value.</returns>
        public static Academy.Shared.Data.Models.Assessments.Enums.AssessmentType ConvertAssessmentType(this Academy.Services.Api.Endpoints.Assessments.AssessmentType value)
        {
            return value switch
            {
                Academy.Services.Api.Endpoints.Assessments.AssessmentType.Quiz => Academy.Shared.Data.Models.Assessments.Enums.AssessmentType.Quiz,
                Academy.Services.Api.Endpoints.Assessments.AssessmentType.Survey => Academy.Shared.Data.Models.Assessments.Enums.AssessmentType.Survey,
                Academy.Services.Api.Endpoints.Assessments.AssessmentType.Exam => Academy.Shared.Data.Models.Assessments.Enums.AssessmentType.Exam,
                _ => throw new ArgumentException($"Invalid assessment type: {value}")
            };
        }

        /// <summary>
        /// Converts a string to an <see cref="Academy.Services.Api.Endpoints.Assessments.AssessmentType"/> enum value.
        /// </summary>
        /// <param name="value">The string value to convert.</param>
        /// <returns>The corresponding <see cref="Academy.Shared.Data.Models.Assessments.Enums.AssessmentType"/> enum value.</returns>
        public static Academy.Services.Api.Endpoints.Assessments.AssessmentType ConvertAssessmentType(this Academy.Shared.Data.Models.Assessments.Enums.AssessmentType value)
        {
            return value switch
            {
                Shared.Data.Models.Assessments.Enums.AssessmentType.Quiz => Academy.Services.Api.Endpoints.Assessments.AssessmentType.Quiz,
                Shared.Data.Models.Assessments.Enums.AssessmentType.Survey => Academy.Services.Api.Endpoints.Assessments.AssessmentType.Survey,
                Shared.Data.Models.Assessments.Enums.AssessmentType.Exam => Academy.Services.Api.Endpoints.Assessments.AssessmentType.Exam,
                _ => throw new ArgumentException($"Invalid assessment type: {value}")
            };
        }
    }
}
