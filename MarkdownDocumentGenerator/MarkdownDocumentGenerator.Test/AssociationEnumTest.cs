using System.Collections;

namespace MarkdownDocumentGenerator.Test
{
    public class AssociationEnumTest(ClassInfoFixture classInfoFixture) : IClassFixture<ClassInfoFixture>
    {
        public record struct ExpectedAssociationEnum(string DisplayTypeName, string DisplayPropertyName);

        public class TestCaseData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[]
                {
                    "DTO.EnumDTO",
                    new ExpectedAssociationEnum[]
                    {
                        new("AlphabetEnum", "FavoriteAlpabet")
                    }
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory]
        [ClassData(typeof(TestCaseData))]
        public void CollectRelatedEnumTest(string fullClassName, ExpectedAssociationEnum[] expectedAssociationEnums)
        {
            var targetClassInfo = classInfoFixture.ClassInfos
                .FirstOrDefault(x => x.FullName == fullClassName);

            Assert.NotNull(targetClassInfo);

            var expected = expectedAssociationEnums
                .Select(x => x.DisplayTypeName)
                .OrderBy(x => x)
                .ToArray();

            var actual = targetClassInfo.AssociationEnums
                .Select(x => x.DisplayName)
                .OrderBy(x => x)
                .ToArray();

            Assert.Equal(expected, actual);
        }
    }
}
