using System.Threading.Tasks;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public partial class SimpleQueryNpgsqlTest
    {
        [ConditionalTheory(Skip = "To be fixed in PG 12.0, https://www.postgresql.org/message-id/CADT4RqAz7oN4vkPir86Kg1_mQBmBxCp-L_%3D9vRpgSNPJf0KRkw%40mail.gmail.com")]
        [MemberData(nameof(IsAsyncData))]
        public override Task Select_with_complex_expression_that_can_be_funcletized(bool isAsync)
            => base.Select_with_complex_expression_that_can_be_funcletized(isAsync);

        public override Task Member_binding_after_ctor_arguments_fails_with_client_eval(bool isAsync)
            => AssertTranslationFailed(() => base.Member_binding_after_ctor_arguments_fails_with_client_eval(isAsync));
    }
}
