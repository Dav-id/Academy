using Academy.Shared.Data.Models.Accounts;
using Academy.Shared.Data.Models.Assessments;
using Academy.Shared.Data.Models.Courses;
using Academy.Shared.Data.Models.Lessons;
using Academy.Shared.Data.Models.Roles;
using Academy.Shared.Data.Models.Tenants;

using Microsoft.EntityFrameworkCore;

namespace Academy.Shared.Data.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected ApplicationDbContext()
        {
        }

        /// <summary>
        /// The ID of the tenant for which the current context is being used.
        /// </summary>
        public long TenantId { get; private set; }

        /// <summary>
        /// Sets the tenant ID for the current context.
        /// </summary>
        public void SetTenant(long tenantId)
        {
            TenantId = tenantId;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Accounts
            modelBuilder.Entity<UserProfile>()
                        .HasQueryFilter(c => !c.IsDeleted && c.TenantId == TenantId);

            // Assessments
            modelBuilder.Entity<Assessment>()
                        .HasQueryFilter(c => !c.IsDeleted && c.TenantId == TenantId);
            modelBuilder.Entity<AssessmentSection>()
                        .HasQueryFilter(c => !c.IsDeleted && c.TenantId == TenantId);
            modelBuilder.Entity<AssessmentSectionQuestion>()
                        .HasQueryFilter(c => !c.IsDeleted && c.TenantId == TenantId);
            modelBuilder.Entity<AssessmentSectionQuestionAnswer>()
                        .HasQueryFilter(c => !c.IsDeleted && c.TenantId == TenantId);
            modelBuilder.Entity<AssessmentSectionQuestionAnswerOption>()
                        .HasQueryFilter(c => !c.IsDeleted && c.TenantId == TenantId);
            modelBuilder.Entity<AssessmentSectionQuestionOption>()
                        .HasQueryFilter(c => !c.IsDeleted && c.TenantId == TenantId);

            //Courses
            modelBuilder.Entity<Course>()
                        .HasQueryFilter(c => !c.IsDeleted && c.TenantId == TenantId);
            modelBuilder.Entity<CourseCompletion>()
                        .HasQueryFilter(c => !c.IsDeleted && c.TenantId == TenantId);
            modelBuilder.Entity<CourseEnrollment>()
                        .HasQueryFilter(c => !c.IsDeleted && c.TenantId == TenantId);
            modelBuilder.Entity<CourseModule>()
                        .HasQueryFilter(c => !c.IsDeleted && c.TenantId == TenantId);

            //Lessons
            modelBuilder.Entity<Lesson>()
                        .HasQueryFilter(c => !c.IsDeleted && c.TenantId == TenantId);
            modelBuilder.Entity<LessonSection>()
                        .HasQueryFilter(c => !c.IsDeleted && c.TenantId == TenantId);
            modelBuilder.Entity<LessonCompletion>()
                        .HasQueryFilter(c => !c.IsDeleted && c.TenantId == TenantId);
            modelBuilder.Entity<LessonSectionContent>()
                        .HasQueryFilter(c => !c.IsDeleted && c.TenantId == TenantId);

            // Composite keys for prerequisite relationships
            modelBuilder.Entity<LessonPrerequisiteAssessment>()
                        .HasKey(lp => new { lp.LessonId, lp.PrerequisiteAssessmentId });

            modelBuilder.Entity<LessonPrerequisiteAssessment>()
                        .HasQueryFilter(c => !c.IsDeleted && c.TenantId == TenantId)
                        .HasOne(lp => lp.Lesson)
                        .WithMany(l => l.PrerequisiteAssessments)
                        .HasForeignKey(lp => lp.LessonId)
                        .OnDelete(DeleteBehavior.Restrict);

            // Composite key for LessonPrerequisiteLesson
            modelBuilder.Entity<LessonPrerequisiteLesson>()
                    .HasKey(lp => new { lp.LessonId, lp.PrerequisiteLessonId });

            modelBuilder.Entity<LessonPrerequisiteLesson>()
                        .HasQueryFilter(c => !c.IsDeleted && c.TenantId == TenantId)
                        .HasOne(lp => lp.Lesson)
                        .WithMany(l => l.PrerequisiteLessons)
                        .HasForeignKey(lp => lp.LessonId)
                        .OnDelete(DeleteBehavior.Restrict);


            //Roles
            modelBuilder.Entity<ExternalRoleMapping>()
                        .HasQueryFilter(c => !c.IsDeleted);

            //Tenants
            modelBuilder.Entity<Tenant>()
                        .HasQueryFilter(c => !c.IsDeleted);
        }

        // Accounts
        public DbSet<UserProfile>                               UserProfiles                                { get; set; }

        // Assessments
        public DbSet<Assessment>                                Assessments                                 { get; set; }
        public DbSet<AssessmentSection>                         AssessmentSections                          { get; set; }
        public DbSet<AssessmentSectionQuestion>                 AssessmentSectionQuestions                  { get; set; }
        public DbSet<AssessmentSectionQuestionAnswer>           AssessmentSectionQuestionAnswers            { get; set; }
        public DbSet<AssessmentSectionQuestionAnswerOption>     AssessmentSectionQuestionAnswerOptions      { get; set; }
        public DbSet<AssessmentSectionQuestionOption>           AssessmentSectionQuestionOptions            { get; set; }

        // Courses
        public DbSet<Course>                                    Courses                                     { get; set; }
        public DbSet<CourseCompletion>                          CourseCompletions                           { get; set; }
        public DbSet<CourseEnrollment>                          CourseEnrollments                           { get; set; }
        public DbSet<CourseModule>                              CourseModules                               { get; set; }

        // Lessons
        public DbSet<Lesson>                                    Lessons                                     { get; set; }
        public DbSet<LessonCompletion>                          LessonCompletions                           { get; set; }
        public DbSet<LessonSection>                             LessonSections                              { get; set; }
        public DbSet<LessonSectionContent>                      LessonSectionContents                       { get; set; }
        public DbSet<LessonPrerequisiteAssessment>              LessonPrerequisiteAssessments               { get; set; }
        public DbSet<LessonPrerequisiteLesson>                  LessonPrerequisiteLessons                   { get; set; }

        // Roles
        public DbSet<ExternalRoleMapping>                       ExternalRoleMappings                        { get; set; }

        // Tenants
        public DbSet<Tenant>                                    Tenants                                     { get; set; }
    }
}
