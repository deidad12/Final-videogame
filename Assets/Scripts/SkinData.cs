using UnityEngine;

[CreateAssetMenu(fileName = "NewSkinData", menuName = "PastelDream/SkinData")]
public class SkinData : ScriptableObject
{
    public string skinName = "Basic Pink";
    public Color colorTint = Color.white;
    public Sprite characterSprite;
    public Sprite accessorySprite;
    public Vector3 accessoryOffset = new Vector3(0f, 0.5f, 0f);
    public Vector3 accessoryScale = Vector3.one;
}
