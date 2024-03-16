using Xunit;

namespace AdvancedSharpAdbClient.Models.Tests
{
    /// <summary>
    /// Tests the <see cref="UnixFileStatusExtensions"/> class.
    /// </summary>
    public class UnixFileStatusExtensionsTests
    {
        [Theory]
        [InlineData((UnixFileStatus)0x7DAB, (UnixFileStatus)0x7000)]
        [InlineData((UnixFileStatus)0x3D94, (UnixFileStatus)0x3000)]
        [InlineData((UnixFileStatus)0x3052, (UnixFileStatus)0x3000)]
        [InlineData((UnixFileStatus)0x8724, (UnixFileStatus)0x8000)]
        [InlineData((UnixFileStatus)0xEBC3, (UnixFileStatus)0xE000)]
        [InlineData((UnixFileStatus)0xBFBA, (UnixFileStatus)0xB000)]
        [InlineData((UnixFileStatus)0x2059, (UnixFileStatus)0x2000)]
        [InlineData((UnixFileStatus)0xAFDC, (UnixFileStatus)0xA000)]
        [InlineData((UnixFileStatus)0x2D2F, (UnixFileStatus)0x2000)]
        [InlineData((UnixFileStatus)0xC10F, (UnixFileStatus)0xC000)]
        public void GetFileTypeTest(UnixFileStatus source, UnixFileStatus type) => Assert.Equal(type, source.GetFileType());

        [Theory]
        [InlineData((UnixFileStatus)0xF118, (UnixFileStatus)0x0118)]
        [InlineData((UnixFileStatus)0x638A, (UnixFileStatus)0x038A)]
        [InlineData((UnixFileStatus)0xB72D, (UnixFileStatus)0x072D)]
        [InlineData((UnixFileStatus)0x5AF9, (UnixFileStatus)0x0AF9)]
        [InlineData((UnixFileStatus)0xA3F5, (UnixFileStatus)0x03F5)]
        [InlineData((UnixFileStatus)0x7ACD, (UnixFileStatus)0x0ACD)]
        [InlineData((UnixFileStatus)0x4751, (UnixFileStatus)0x0751)]
        [InlineData((UnixFileStatus)0xDA30, (UnixFileStatus)0x0A30)]
        [InlineData((UnixFileStatus)0xB829, (UnixFileStatus)0x0829)]
        [InlineData((UnixFileStatus)0xFE1A, (UnixFileStatus)0x0E1A)]
        public void GetPermissionsTest(UnixFileStatus source, UnixFileStatus type) => Assert.Equal(type, source.GetPermissions());

        [Theory]
        [InlineData((UnixFileStatus)0x28DA, (UnixFileStatus)0x00DA)]
        [InlineData((UnixFileStatus)0x69EE, (UnixFileStatus)0x01EE)]
        [InlineData((UnixFileStatus)0x2507, (UnixFileStatus)0x0107)]
        [InlineData((UnixFileStatus)0x763F, (UnixFileStatus)0x003F)]
        [InlineData((UnixFileStatus)0x8ECC, (UnixFileStatus)0x00CC)]
        [InlineData((UnixFileStatus)0xBFB8, (UnixFileStatus)0x01B8)]
        [InlineData((UnixFileStatus)0xF893, (UnixFileStatus)0x0093)]
        [InlineData((UnixFileStatus)0x8E54, (UnixFileStatus)0x0054)]
        [InlineData((UnixFileStatus)0x6270, (UnixFileStatus)0x0070)]
        [InlineData((UnixFileStatus)0x21AB, (UnixFileStatus)0x01AB)]
        public void GetAccessPermissionsTest(UnixFileStatus source, UnixFileStatus type) => Assert.Equal(type, source.GetAccessPermissions());

        [Theory]
        [InlineData((UnixFileStatus)0x0FFF, false)]
        [InlineData((UnixFileStatus)0x1FB6, false)]
        [InlineData((UnixFileStatus)0x21FF, false)]
        [InlineData((UnixFileStatus)0x41B6, true)]
        [InlineData((UnixFileStatus)0x616D, false)]
        [InlineData((UnixFileStatus)0x80DB, false)]
        [InlineData((UnixFileStatus)0xA124, false)]
        [InlineData((UnixFileStatus)0xC092, false)]
        [InlineData((UnixFileStatus)0xF049, false)]
        public void IsDirectoryTest(UnixFileStatus mode, bool result) => Assert.Equal(result, mode.IsDirectory());

        [Theory]
        [InlineData((UnixFileStatus)0x0FFF, false)]
        [InlineData((UnixFileStatus)0x1FB6, false)]
        [InlineData((UnixFileStatus)0x21FF, true)]
        [InlineData((UnixFileStatus)0x41B6, false)]
        [InlineData((UnixFileStatus)0x616D, false)]
        [InlineData((UnixFileStatus)0x80DB, false)]
        [InlineData((UnixFileStatus)0xA124, false)]
        [InlineData((UnixFileStatus)0xC092, false)]
        [InlineData((UnixFileStatus)0xF049, false)]
        public void IsCharacterFileTest(UnixFileStatus mode, bool result) => Assert.Equal(result, mode.IsCharacterFile());

        [Theory]
        [InlineData((UnixFileStatus)0x0FFF, false)]
        [InlineData((UnixFileStatus)0x1FB6, false)]
        [InlineData((UnixFileStatus)0x21FF, false)]
        [InlineData((UnixFileStatus)0x41B6, false)]
        [InlineData((UnixFileStatus)0x616D, true)]
        [InlineData((UnixFileStatus)0x80DB, false)]
        [InlineData((UnixFileStatus)0xA124, false)]
        [InlineData((UnixFileStatus)0xC092, false)]
        [InlineData((UnixFileStatus)0xF049, false)]
        public void IsBlockFileTest(UnixFileStatus mode, bool result) => Assert.Equal(result, mode.IsBlockFile());

        [Theory]
        [InlineData((UnixFileStatus)0x0FFF, false)]
        [InlineData((UnixFileStatus)0x1FB6, false)]
        [InlineData((UnixFileStatus)0x21FF, false)]
        [InlineData((UnixFileStatus)0x41B6, false)]
        [InlineData((UnixFileStatus)0x616D, false)]
        [InlineData((UnixFileStatus)0x80DB, true)]
        [InlineData((UnixFileStatus)0xA124, false)]
        [InlineData((UnixFileStatus)0xC092, false)]
        [InlineData((UnixFileStatus)0xF049, false)]
        public void IsRegularFileTest(UnixFileStatus mode, bool result) => Assert.Equal(result, mode.IsRegularFile());

        [Theory]
        [InlineData((UnixFileStatus)0x0FFF, false)]
        [InlineData((UnixFileStatus)0x1FB6, true)]
        [InlineData((UnixFileStatus)0x21FF, false)]
        [InlineData((UnixFileStatus)0x41B6, false)]
        [InlineData((UnixFileStatus)0x616D, false)]
        [InlineData((UnixFileStatus)0x80DB, false)]
        [InlineData((UnixFileStatus)0xA124, false)]
        [InlineData((UnixFileStatus)0xC092, false)]
        [InlineData((UnixFileStatus)0xF049, false)]
        public void IsFIFOTest(UnixFileStatus mode, bool result) => Assert.Equal(result, mode.IsFIFO());

        [Theory]
        [InlineData((UnixFileStatus)0x0FFF, false)]
        [InlineData((UnixFileStatus)0x1FB6, false)]
        [InlineData((UnixFileStatus)0x21FF, false)]
        [InlineData((UnixFileStatus)0x41B6, false)]
        [InlineData((UnixFileStatus)0x616D, false)]
        [InlineData((UnixFileStatus)0x80DB, false)]
        [InlineData((UnixFileStatus)0xA124, true)]
        [InlineData((UnixFileStatus)0xC092, false)]
        [InlineData((UnixFileStatus)0xF049, false)]
        public void IsSymbolicLinkTest(UnixFileStatus mode, bool result) => Assert.Equal(result, mode.IsSymbolicLink());

        [Theory]
        [InlineData((UnixFileStatus)0x0FFF, false)]
        [InlineData((UnixFileStatus)0x1FB6, false)]
        [InlineData((UnixFileStatus)0x21FF, false)]
        [InlineData((UnixFileStatus)0x41B6, false)]
        [InlineData((UnixFileStatus)0x616D, false)]
        [InlineData((UnixFileStatus)0x80DB, false)]
        [InlineData((UnixFileStatus)0xA124, false)]
        [InlineData((UnixFileStatus)0xC092, true)]
        [InlineData((UnixFileStatus)0xF049, false)]
        public void IsSocketTest(UnixFileStatus mode, bool result) => Assert.Equal(result, mode.IsSocket());

        [Theory]
        [InlineData((UnixFileStatus)0x0FFF, true)]
        [InlineData((UnixFileStatus)0x1FB6, true)]
        [InlineData((UnixFileStatus)0x21FF, true)]
        [InlineData((UnixFileStatus)0x41B6, false)]
        [InlineData((UnixFileStatus)0x616D, true)]
        [InlineData((UnixFileStatus)0x80DB, false)]
        [InlineData((UnixFileStatus)0xA124, false)]
        [InlineData((UnixFileStatus)0xC092, true)]
        [InlineData((UnixFileStatus)0xF049, true)]
        public void IsOtherTest(UnixFileStatus mode, bool result) => Assert.Equal(result, mode.IsOther());

        [Theory]
        [InlineData((UnixFileStatus)0x0FFF, false)]
        [InlineData((UnixFileStatus)0x1FB6, true)]
        [InlineData((UnixFileStatus)0x21FF, true)]
        [InlineData((UnixFileStatus)0x41B6, true)]
        [InlineData((UnixFileStatus)0x616D, true)]
        [InlineData((UnixFileStatus)0x80DB, true)]
        [InlineData((UnixFileStatus)0xA124, true)]
        [InlineData((UnixFileStatus)0xC092, true)]
        [InlineData((UnixFileStatus)0xF049, true)]
        public void IsTypeKnownTest(UnixFileStatus mode, bool result) => Assert.Equal(result, mode.IsTypeKnown());

        [Theory]
        [InlineData("\0rwsrwsrwt", (UnixFileStatus)0x0FFF)]
        [InlineData("prwSrwSrwT", (UnixFileStatus)0x1FB6)]
        [InlineData("crwxrwxrwx", (UnixFileStatus)0x21FF)]
        [InlineData("drw-rw-rw-", (UnixFileStatus)0x41B6)]
        [InlineData("br-xr-xr-x", (UnixFileStatus)0x616D)]
        [InlineData("--wx-wx-wx", (UnixFileStatus)0x80DB)]
        [InlineData("lr--r--r--", (UnixFileStatus)0xA124)]
        [InlineData("s-w--w--w-", (UnixFileStatus)0xC092)]
        [InlineData("?--x--x--x", (UnixFileStatus)0x0049)]
        [InlineData("7777", (UnixFileStatus)0x0FFF)]
        public void FromPermissionCodeTest(string code, UnixFileStatus mode) => Assert.Equal(mode, UnixFileStatusExtensions.FromPermissionCode(code));

        [Theory]
        [InlineData((UnixFileStatus)0x0FFF, "\0rwsrwsrwt")]
        [InlineData((UnixFileStatus)0x1FB6, "prwSrwSrwT")]
        [InlineData((UnixFileStatus)0x21FF, "crwxrwxrwx")]
        [InlineData((UnixFileStatus)0x41B6, "drw-rw-rw-")]
        [InlineData((UnixFileStatus)0x616D, "br-xr-xr-x")]
        [InlineData((UnixFileStatus)0x80DB, "--wx-wx-wx")]
        [InlineData((UnixFileStatus)0xA124, "lr--r--r--")]
        [InlineData((UnixFileStatus)0xC092, "s-w--w--w-")]
        [InlineData((UnixFileStatus)0xF049, "?--x--x--x")]
        public void ToPermissionCodeTest(UnixFileStatus mode, string code) => Assert.Equal(code, mode.ToPermissionCode());
    }
}
