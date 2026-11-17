using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using EcoGuardian.Core;
namespace EcoGuardian.Tests
{
    public class SceneSmokeTests
    {
        string previousSaveDirectory,tempSaveDirectory;
        [UnitySetUp]public IEnumerator IsolatedProgress()
        {
            previousSaveDirectory=Environment.GetEnvironmentVariable("ECO_GUARDIAN_SAVE_DIR");
            tempSaveDirectory=Path.Combine(Application.temporaryCachePath,"EcoGuardianTests",Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempSaveDirectory);Environment.SetEnvironmentVariable("ECO_GUARDIAN_SAVE_DIR",tempSaveDirectory);
            yield return SceneManager.LoadSceneAsync("MainMenu");yield return null;
        }
        [UnityTearDown]public IEnumerator RestoreProgress()
        {
            var app=UnityEngine.Object.FindObjectOfType<GuardianGame>();if(app!=null)UnityEngine.Object.Destroy(app.gameObject);
            yield return null;Environment.SetEnvironmentVariable("ECO_GUARDIAN_SAVE_DIR",previousSaveDirectory);
            if(Directory.Exists(tempSaveDirectory))Directory.Delete(tempSaveDirectory,true);
        }
        [UnityTest]public IEnumerator MenuLoadsAndCampaignStarts()
        {
            var app=UnityEngine.Object.FindObjectOfType<GuardianGame>();Assert.NotNull(app);Assert.NotNull(Camera.main);Assert.AreEqual(2,app.GetComponents<AudioSource>().Length);
            app.StartStage(1);yield return null;
            Assert.AreEqual(100,app.Session.Hp);Assert.AreEqual(150,app.Session.Energy);Assert.AreEqual(0,app.Session.Wave);
            float elapsed=app.Session.Elapsed;yield return null;Assert.AreEqual(elapsed,app.Session.Elapsed);
            Assert.IsFalse(File.Exists(Path.Combine(tempSaveDirectory,"eco-guardian-save.json")),"Starting a stage must not create a progress save");
        }
        [UnityTest]public IEnumerator LockedStageCannotBeStarted()
        {
            var app=UnityEngine.Object.FindObjectOfType<GuardianGame>();var previous=app.Session;app.StartStage(2);yield return null;
            Assert.AreSame(previous,app.Session);Assert.AreEqual(1,app.Session.Level.index);
        }
        [UnityTest]public IEnumerator RetryRecreatesCleanSession()
        {
            var app=UnityEngine.Object.FindObjectOfType<GuardianGame>();app.StartStage(1);var previous=app.Session;
            previous.Plant(TowerKind.Thorn,previous.Level.pads.FindIndex(p=>!p.trap&&!p.polluted));previous.StartWave();previous.Upgrade(previous.Towers[0]);
            Assert.Less(previous.Energy,150);app.StartStage(1);yield return null;
            Assert.AreNotSame(previous,app.Session);Assert.AreEqual(150,app.Session.Energy);Assert.AreEqual(0,app.Session.Towers.Count);Assert.AreEqual(0,app.Session.Enemies.Count);Assert.AreEqual(0,app.Session.Wave);
        }
        [UnityTest]public IEnumerator LevelSelectEntrySceneLoads()
        {
            yield return SceneManager.LoadSceneAsync("LevelSelect");yield return null;
            var app=UnityEngine.Object.FindObjectOfType<GuardianGame>();Assert.NotNull(app);Assert.AreEqual("levels",app.entryScreen);Assert.NotNull(Camera.main);
        }
    }
}
