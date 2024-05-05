using System.Collections;

namespace MarkdownDocumentGenerator.Test
{
    public class AssociationEnumTest(TypeInfoFixture typeInfoFixture) : IClassFixture<TypeInfoFixture>
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
                        new("AlphabetEnum", "FavoriteAlpabet"),
                        new("NumberEnum", "FavoriteNumber"),
                    }
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory]
        [ClassData(typeof(TestCaseData))]
        public void CollectRelatedEnumTest(string fullTypeName, ExpectedAssociationEnum[] expectedAssociationEnums)
        {
            var targetTypeInfo = typeInfoFixture.TypeInfos
                .FirstOrDefault(x => x.FullName == fullTypeName);

            Assert.NotNull(targetTypeInfo);

            var expected = expectedAssociationEnums
                .Select(x => x.DisplayTypeName)
                .OrderBy(x => x)
                .ToArray();

            var actual = targetTypeInfo.AssociationEnums
                .Select(x => x.DisplayName)
                .OrderBy(x => x)
                .ToArray();

            Assert.Equal(expected, actual);
        }
    }
}
