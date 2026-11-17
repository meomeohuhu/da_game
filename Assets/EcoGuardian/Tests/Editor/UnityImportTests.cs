using NUnit.Framework;
using UnityEngine;
namespace EcoGuardian.Tests
{
    public class UnityImportTests
    {
        [TestCase("thorn")][TestCase("frost")][TestCase("bloom")][TestCase("bomb")][TestCase("poison")][TestCase("ancient")]
        public void ArtworkImports(string name){Assert.NotNull(Resources.Load<Texture2D>("Art/"+name));}
        [TestCase("forest")][TestCase("hit")][TestCase("plant")][TestCase("wave")]
        public void AudioImports(string name){Assert.NotNull(Resources.Load<AudioClip>("Audio/"+name));}
        [Test]public void VietnameseFontImports(){Assert.NotNull(Resources.Load<Font>("Fonts/GuardianSans"));}
    }
}
