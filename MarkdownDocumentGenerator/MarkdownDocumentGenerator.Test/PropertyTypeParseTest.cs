namespace MarkdownDocumentGenerator.Test
{
    public class PropertyTypeParseTest(TypeInfoFixture typeInfoFixture) : IClassFixture<TypeInfoFixture>
    {
        [Theory]
        [InlineData("DTO.Player", "Name", "string")]
        [InlineData("DTO.Player", "Hp", "int")]
        [InlineData("DTO.GameInfo", "Title", "string")]
        [InlineData("DTO.GameInfo", "PlayersArray", "Player[]")]
        [InlineData("DTO.GameInfo", "PlayersList", "List<Player>")]
        [InlineData("DTO.GameInfo", "Item1", "Item1?")]
        [InlineData("DTO.GameInfo", "Item2", "Item2?")]
        [InlineData("DTO.GameInfo", "Item3", "Item3?")]
        [InlineData("DTO.RequestFood", "FoodName", "string")]
        [InlineData("DTO.GodPlayer", "Rank", "int")]
        [InlineData("DTO.GodPlayer", "CurrentMoveType", "MoveType")]
        [InlineData("DTO.CircularReferencePlayer", "Parent", "CircularReferencePlayer?")]
        [InlineData("DTO.CircularReferencePlayer", "Children", "CircularReferencePlayer[]")]
        [InlineData("DTO.GodGameInfo", "WorldId", "int")]
        [InlineData("DTO.NullableDTO", "NullableInt", "int?")]
        [InlineData("DTO.NullableDTO", "NullableDateTime", "DateTime?")]
        [InlineData("DTO.NullableDTO", "NullableString", "string?")]
        [InlineData("DTO.NullableDTO", "NullableItem1", "Item1?")]
        [InlineData("DTO.NullableDTO", "IntArray", "int[]")]
        [InlineData("DTO.NullableDTO", "IntNullableArray", "int[]?")]
        [InlineData("DTO.NullableDTO", "NullableIntArray", "int?[]")]
        [InlineData("DTO.NullableDTO", "NullableIntNullableArray", "int?[]?")]
        [InlineData("DTO.NullableDTO", "IntList", "List<int>")]
        [InlineData("DTO.NullableDTO", "IntNullableList", "List<int>?")]
        [InlineData("DTO.NullableDTO", "NullableIntList", "List<int?>")]
        [InlineData("DTO.NullableDTO", "NullableIntNullableList", "List<int?>?")]
        [InlineData("DTO.NullableDTO", "NullableIntNullableListNullableList", "List<List<int?>?>?")]
        [InlineData("DTO.DictionaryDTO", "Players", "Dictionary<int, Player>")]
        public void PropertyTypeParse(string fullTypeName, string propertyDisplayName, string expectedDisplayTypeName)
        {
            var targetTypeInfo = typeInfoFixture.TypeInfos
                .FirstOrDefault(x => x.FullName == fullTypeName);

            Assert.NotNull(targetTypeInfo);

            var targetPropertyInfo = targetTypeInfo.Properties
                .FirstOrDefault(x => x.DisplayName == propertyDisplayName);

            Assert.NotNull(targetPropertyInfo);

            Assert.Equal(expectedDisplayTypeName, targetPropertyInfo.DisplayTypeName);
        }
    }
}
