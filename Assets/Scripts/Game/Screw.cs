using UnityEngine;

public class Screw : MonoBehaviour
{
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private Transform _questionMarkPlace;
    [SerializeField] private GameObject _questionMark;

    private GameObject _spawnedQuestionMark;
    private Material _materail;
    private int _index;
    private bool _isSecret;

    public int Index => _index;
    public bool IsSecret => _isSecret;

    public void RevealMaterial()
    {
        if (_spawnedQuestionMark == null)
            return;

        Destroy(_spawnedQuestionMark);
        _isSecret = false;
        _meshRenderer.material = _materail; 
    }

    public void SetInfo(int index, Material material, Material secretMaterial = null)
    {
        _index = index;
        _materail = material;

        if (secretMaterial == null)
        {
            _meshRenderer.material = material;
        }
        else
        {
            if (_spawnedQuestionMark != null)
                Destroy(_spawnedQuestionMark);

            _isSecret = true;
            _spawnedQuestionMark = Instantiate(_questionMark, _questionMarkPlace.position, _questionMarkPlace.rotation, transform);
            _meshRenderer.material = secretMaterial;
        }
    }
}