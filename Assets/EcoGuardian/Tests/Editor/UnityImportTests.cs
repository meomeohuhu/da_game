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
        [TestCase(1)][TestCase(2)][TestCase(3)][TestCase(4)][TestCase(5)]
        public void EditableLevelAssetLoads(int stage){var level=Resources.Load<LevelData>("Levels/Level"+stage.ToString("00"));Assert.NotNull(level);Assert.AreEqual(stage,level.configuration.index);Assert.AreEqual(5,level.configuration.waves.Count);Assert.AreEqual(8+(stage-1)*2,level.configuration.pads.FindAll(p=>!p.polluted).Count);}
        [Test]public void VietnameseFontImports(){Assert.NotNull(Resources.Load<Font>("Fonts/GuardianSans"));}
    }
}
