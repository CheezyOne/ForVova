using UnityEngine;

[CreateAssetMenu(fileName = "ScrewsConfig", menuName = "Avlerm/Screws configuration")]
public class ScrewsConfig : ScriptableObject
{
    [SerializeField] private Material[] _materials;
    [SerializeField] private Material _secretMaterial;

    public Material[] Materials => _materials;
    public Material SecretMaterial => _secretMaterial;
}