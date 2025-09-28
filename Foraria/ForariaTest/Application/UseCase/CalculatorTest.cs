using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foraria.Application.UseCase;

namespace ForariaTest.Application.UseCase
{
    public class CalculatorTest
    {
        [Fact]
        public void Sum_ShouldReturnCorrectResult()
        {
            // Arrange
            var calculator = new Calculator();

            // Act
            var result = calculator.Sum(2, 3);

            // Assert
            Assert.Equal(5, result);
        }
    }
}

