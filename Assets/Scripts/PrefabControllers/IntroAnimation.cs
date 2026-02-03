using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class IntroAnimation : MonoBehaviour
{
    private void OnEnable()
    {
        var document = GetComponent<UIDocument>();
        var root = document.rootVisualElement;
        var logo = root.Q("logo");
        var background = root.Q("Background");
        var playButton = background.Q<Button>("PlayButton");
        var quitButton = background.Q<Button>("QuitButton");
        
        playButton.clicked += Play;
        quitButton.clicked += Quit;
        
        root.schedule.Execute(() =>
        {
            logo.RegisterCallbackOnce<TransitionEndEvent>(evt =>
            {
                Shake(logo, 8, 12);
            });
            logo.schedule.Execute(() => { logo.RemoveFromClassList("logo-hidden"); });
            
            background.schedule.Execute(() => { background.RemoveFromClassList("background-hidden"); });

            var i = 0;
            foreach (var child in background.Children())
            {
                child.schedule.Execute(() => { child.RemoveFromClassList("button-hidden"); }).ExecuteLater(i * 200);
                i++;
            }
        });
    }

    private void Play()
    {
        SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
    }

    private void Quit()
    {
        Application.Quit(0);
    }
    
    void Shake(VisualElement el, int shakes = 6, float strength = 6f)
    {
        var i = 0;

        void Step()
        {
            var offset = Random.insideUnitCircle * strength;
            el.style.translate = new Translate(offset.x, offset.y, 0);

            i++;
            if (i < shakes)
            {
                el.schedule.Execute(Step).ExecuteLater(60);
            }
            else
            {
                el.style.translate = Translate.None();
            }
        }

        el.AddToClassList("shake");
        Step();
    }

    private void Start()
    {
        var document = GetComponent<UIDocument>();
        var root = document.rootVisualElement;
        var logo = root.Q("logo");
        var background = root.Q("Background");
        logo.schedule.Execute(() =>
        {
            logo.AddToClassList("logo-hidden");
        });
        
        background.schedule.Execute(() =>
        {
            background.AddToClassList("background-hidden");    
        });
        
        foreach (var child in background.Children())
        {
            child.schedule.Execute(() =>
            {
                child.AddToClassList("button-hidden");
            });
        }
    }
    
    void Awake()
    {
        QualitySettings.vSyncCount = 0;

        Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.numerator / (int)Screen.currentResolution.refreshRateRatio.denominator;
    }
}
