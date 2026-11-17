using NUnit.Framework;
using UnityEngine;
using EcoGuardian.Core;
namespace EcoGuardian.Tests
{
    public class SaveSerializationTests
    {
        [Test]public void JsonRoundTripKeepsBestStarsAndPurchases()
        {
            var save=new SaveData();save.Complete(1,3);save.Complete(1,1);save.Buy(0);
            var loaded=JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(save));loaded.Sanitize();
            Assert.AreEqual(3,loaded.stars[0]);Assert.IsTrue(loaded.upgrades[0]);Assert.AreEqual(0,loaded.Available);Assert.AreEqual(2,loaded.unlocked);
        }
    }
}
