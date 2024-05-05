namespace MarkdownDocumentGenerator.Test
{
    public class GenericAssocitaionTest(TypeInfoFixture typeInfoFixture) : IClassFixture<TypeInfoFixture>
    {
        [Theory]
        [InlineData("DTO.ListAssociationDTO", "Player")]
        [InlineData("DTO.ArrayAssociationDTO", "Player")]
        public void CollectedGenericAccotiaionTypes(string fullTypeName, string expectedAssociationTypeName)
        {
            var targetTypeInfo = typeInfoFixture.TypeInfos
                .FirstOrDefault(x => x.FullName == fullTypeName);

            Assert.NotNull(targetTypeInfo);

            Assert.Single(targetTypeInfo.AssociationTypes);

            var associationType = targetTypeInfo.AssociationTypes.First();

            Assert.Equal(expectedAssociationTypeName, associationType.DisplayName);
        }
    }
}
