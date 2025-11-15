using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForariaDomain.Application.UseCase;

namespace ForariaTest.Unit
{
    public class ResetTokenGeneratorPasswordTests
    {
        [Fact]
        public void Generate_ShouldReturnNonNullAndNonEmptyString()
        {
            var generator = new ResetTokenGeneratorPassword();

            var token = generator.Generate();

            
            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public void Generate_ShouldNotContainInvalidCharacters()
        {
            var generator = new ResetTokenGeneratorPassword();

            var token = generator.Generate();

            Assert.DoesNotContain("+", token);
            Assert.DoesNotContain("/", token);
            Assert.DoesNotContain("=", token);
        }

        [Fact]
        public void Generate_ShouldReturnDifferentValues_OnMultipleCalls()
        {
            var generator = new ResetTokenGeneratorPassword();

            var token1 = generator.Generate();
            var token2 = generator.Generate();

            Assert.NotEqual(token1, token2);
        }

        [Fact]
        public void Generate_ShouldReturnTokenOfExpectedLength()
        {
            var generator = new ResetTokenGeneratorPassword();

            var token = generator.Generate();

            Assert.InRange(token.Length, 43, 44);
        }
    }
}
