﻿namespace MarkdownDocumentGenerator.Test
{
    public class GenericAssocitaionTest(ClassInfoFixture classInfoFixture) : IClassFixture<ClassInfoFixture>
    {
        [Theory]
        [InlineData("DTO.ListAssociationDTO", "Player")]
        [InlineData("DTO.ArrayAssociationDTO", "Player")]
        public void CollectedGenericAccotiaionClasses(string fullClassName, string expectedAssociationClassName)
        {
            var targetClassInfo = classInfoFixture.ClassInfos
                .FirstOrDefault(x => x.FullName == fullClassName);

            Assert.NotNull(targetClassInfo);

            Assert.Single(targetClassInfo.AssociationTypes);

            var associationClass = targetClassInfo.AssociationTypes.First();

            Assert.Equal(expectedAssociationClassName, associationClass.DisplayName);
        }
    }
}
