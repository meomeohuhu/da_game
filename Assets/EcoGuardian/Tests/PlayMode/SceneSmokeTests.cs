using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using EcoGuardian.Core;
namespace EcoGuardian.Tests
{
    public class SceneSmokeTests
    {
        [UnityTest]public IEnumerator MenuLoadsAndCampaignStarts()
        {
            yield return SceneManager.LoadSceneAsync("MainMenu");yield return null;
            var app=Object.FindObjectOfType<GuardianGame>();Assert.NotNull(app);Assert.NotNull(Camera.main);Assert.AreEqual(2,app.GetComponents<AudioSource>().Length);
            app.StartStage(1);yield return null;
            Assert.AreEqual(100,app.Session.Hp);Assert.AreEqual(150,app.Session.Energy);Assert.AreEqual(0,app.Session.Wave);
            // No progression past the tutorial until the player performs its action.
            float elapsed=app.Session.Elapsed;yield return null;Assert.AreEqual(elapsed,app.Session.Elapsed);
        }
    }
}
