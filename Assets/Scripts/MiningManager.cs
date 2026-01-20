using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class MiningManager : MonoBehaviour
{
    public GameObject moneyEffectPrefab;
    public GameObject moneyCounter;
    public AudioClip moneyIncrementAudio;
    
    private TilemapManager _manager;
    private RectTransform _moneyCounterRT;
    private float _money = 0.0f;
    private TextMeshProUGUI _moneyText;
    private AudioSource _audioSource;
    
    void Start()
    {
        _manager = FindFirstObjectByType<TilemapManager>();
        _manager.OnTileDamaged += OnTileDestroyed;
        _moneyCounterRT = moneyCounter.GetComponent<RectTransform>();
        _moneyText = moneyCounter.GetComponent<TextMeshProUGUI>();
        
        _audioSource = transform.AddComponent<AudioSource>();
        _audioSource.clip = moneyIncrementAudio;
        _audioSource.pitch = 1f + Random.Range(-0.1f, 0.1f);
    }
    
    private Vector3 GetMoneyPosition()
    {
        Vector3[] corners = new Vector3[4];
        _moneyCounterRT.GetWorldCorners(corners);
        return (corners[0] + corners[1]) * 0.5f + Vector3.right;
    }
    
    public void AddMoney(float amount)
    {
        _money += amount;
        
        _audioSource.Play();
        LeanTween.cancel(moneyCounter);
        moneyCounter.transform.localScale = new Vector3(0.8f, 0.8f, 1);
        moneyCounter.transform.Rotate(Vector3.forward, 20);
        _moneyText.text = "$" + Mathf.FloorToInt(_money);
        LeanTween.scale(moneyCounter, new Vector3(1, 1, 1), 0.2f).setEase(LeanTweenType.easeInOutBack);
        LeanTween.rotateZ(moneyCounter, 0, 0.2f).setEase(LeanTweenType.easeInOutBack);
    }

    void OnTileDestroyed(Vector2Int pos, TileData data)
    {
        if (!_manager.IsTileDead(pos)) return;
        var instance = Instantiate(moneyEffectPrefab, transform);
        var worldPos = _manager.CellToWorld(pos);
        instance.transform.position = worldPos;
        var start = instance.transform.position;
        LeanTween.value(instance, 0.0f, 1.0f, 1.0f).setEase(LeanTweenType.easeInOutCubic).setOnComplete(() =>
        {
            Destroy(instance);
            AddMoney(data.Material.value);
        }).setOnUpdate(t =>
        {
            instance.transform.position = Vector3.LerpUnclamped(start, GetMoneyPosition(), t);
        });
    }
}
