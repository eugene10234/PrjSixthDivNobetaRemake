using NUnit.Framework;
using UnityEngine;


namespace Assets.Tests.EditMode.Sample
{
    public class SimpleMathTest
    {
        [Test]
        public void TwoPlusTwo_EqualsFour()
        {
            // Arrange (準備)
            int a = 2;
            int b = 2;

            // Act (執行)
            int result = a + b;

            // Assert (斷言/驗證結果)
            Assert.AreEqual(4, result);
        }
        [Test]
        [TestCase(1f, 0, 1f)]
        [TestCase(0, 1f, 0)]
        public void DotTest(float x,float y,float z)
        {
            // Arrange (準備)
            Vector2 a = new Vector2(1f,0);
            Vector2 b = new Vector2(x,y);

            // Act (執行)
            float result = Vector2.Dot(a,b);

            // Assert (斷言/驗證結果)
            Assert.AreEqual(z, result);
        }
    }
}
