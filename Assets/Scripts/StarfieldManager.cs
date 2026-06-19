using UnityEngine;

public class StarfieldManager : MonoBehaviour
{

    [Header("─── Yıldız Ayarları ───")]
    [Tooltip("Toplam yıldız sayısı. HTML prototipinde 80.")]
    [SerializeField, Range(30, 200)]
    private int _starCount = 80;

    [Tooltip("Yıldız rengi. HTML'de #c8d6e5 (açık gri-mavi).")]
    [SerializeField]
    private Color _starColor = new Color(0.784f, 0.839f, 0.898f, 1f); // #c8d6e5

    [Header("─── Hız (World Units/Saniye) ───")]
    [Tooltip("En yavaş yıldız hızı (uzak yıldızlar).")]
    [SerializeField]
    private float _minSpeed = 0.8f;

    [Tooltip("En hızlı yıldız hızı (yakın yıldızlar).")]
    [SerializeField]
    private float _maxSpeed = 2.5f;

    [Header("─── Boyut (World Units) ───")]
    [Tooltip("En küçük yıldız boyutu.")]
    [SerializeField]
    private float _minSize = 0.01f;

    [Tooltip("En büyük yıldız boyutu.")]
    [SerializeField]
    private float _maxSize = 0.04f;

    [Header("─── Parlaklık ───")]
    [Tooltip("En sönük yıldız alpha'sı (uzak).")]
    [SerializeField, Range(0.1f, 1f)]
    private float _minAlpha = 0.3f;

    [Tooltip("En parlak yıldız alpha'sı (yakın).")]
    [SerializeField, Range(0.1f, 1f)]
    private float _maxAlpha = 0.9f;

    [Header("─── Rendering ───")]
    [Tooltip("Sorting Order. Negatif = gameplay'in arkasında.\n" +
             "-10 önerilir. Player/Enemy genellikle 0'da.")]
    [SerializeField]
    private int _sortingOrder = -10;

       private struct StarData
    {
        public float speed;  
        public float size;   
        public float alpha;   
    }


    private ParticleSystem _particleSystem;
    private ParticleSystem.Particle[] _particles;  
    private StarData[] _starData;                   

    private Camera _mainCamera;
    private float _screenMinX, _screenMaxX;
    private float _screenMinY, _screenMaxY;


    private void Awake()
    {
        _mainCamera = Camera.main;
        CalculateScreenBounds();
        CreateParticleSystem();
        InitializeStars();
    }

    private void Update()
    {
        
        float dt = Time.unscaledDeltaTime;

        for (int i = 0; i < _starCount; i++)
        {
            
            Vector3 pos = _particles[i].position;
            pos.y -= _starData[i].speed * dt;

            
            if (pos.y < _screenMinY - 0.5f)
            {
                pos.y = _screenMaxY + 0.5f;
                pos.x = Random.Range(_screenMinX, _screenMaxX);
            }

            _particles[i].position = pos;
        }

        _particleSystem.SetParticles(_particles, _starCount);
    }

    private void CalculateScreenBounds()
    {
        float zDist = Mathf.Abs(_mainCamera.transform.position.z);

        Vector3 bottomLeft = _mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, zDist));
        Vector3 topRight   = _mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, zDist));

        _screenMinX = bottomLeft.x;
        _screenMaxX = topRight.x;
        _screenMinY = bottomLeft.y;
        _screenMaxY = topRight.y;
    }

    private void CreateParticleSystem()
    {
        
        GameObject go = new GameObject("Starfield");
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;

        _particleSystem = go.AddComponent<ParticleSystem>();

        
        var main = _particleSystem.main;
        main.maxParticles    = _starCount;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.playOnAwake     = false;
        main.loop            = true;
        main.useUnscaledTime = true;

        
        main.startLifetime = 9999f;
        main.startSpeed    = 0f;
        main.startSize     = 1f; 

        
        var emission = _particleSystem.emission;
        emission.enabled = false;

        
        var shape = _particleSystem.shape;
        shape.enabled = false;

        
        var vel = _particleSystem.velocityOverLifetime;
        vel.enabled = false;

        var col = _particleSystem.colorOverLifetime;
        col.enabled = false;

        var sizeOL = _particleSystem.sizeOverLifetime;
        sizeOL.enabled = false;

        var noise = _particleSystem.noise;
        noise.enabled = false;

        
        ParticleSystemRenderer rend = _particleSystem.GetComponent<ParticleSystemRenderer>();
        rend.sortingOrder = _sortingOrder;
        rend.renderMode   = ParticleSystemRenderMode.Billboard;

        rend.material = Resources.GetBuiltinResource<Material>("Sprites-Default.mat");

        
        _particleSystem.Play();
    }

    private void InitializeStars()
    {
        _particles = new ParticleSystem.Particle[_starCount];
        _starData  = new StarData[_starCount];

        for (int i = 0; i < _starCount; i++)
        {
            
            float speed = Random.Range(_minSpeed, _maxSpeed);

            float depthFactor = Mathf.InverseLerp(_minSpeed, _maxSpeed, speed);

            float size = Mathf.Lerp(_minSize, _maxSize, depthFactor)
                       + Random.Range(-0.005f, 0.005f);
            size = Mathf.Max(0.005f, size);

            float alpha = Mathf.Lerp(_minAlpha, _maxAlpha, depthFactor)
                        + Random.Range(-0.1f, 0.1f);
            alpha = Mathf.Clamp(alpha, _minAlpha, _maxAlpha);

            _starData[i] = new StarData
            {
                speed = speed,
                size  = size,
                alpha = alpha
            };

            _particles[i].position      = new Vector3(
                Random.Range(_screenMinX, _screenMaxX),
                Random.Range(_screenMinY, _screenMaxY),
                0f
            );
            _particles[i].startSize     = size;
            _particles[i].startColor    = new Color32(
                (byte)(_starColor.r * 255),
                (byte)(_starColor.g * 255),
                (byte)(_starColor.b * 255),
                (byte)(alpha * 255)
            );
            _particles[i].startLifetime = 9999f;
            _particles[i].remainingLifetime = 9999f;
        }

        _particleSystem.SetParticles(_particles, _starCount);
    }
}