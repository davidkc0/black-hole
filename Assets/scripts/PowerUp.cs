using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum PowerUpType { ChangeColor, Freeze }
    public PowerUpType type;
    public float duration = 10f;
    public SpriteRenderer spriteRenderer;
    public Sprite changeColorSprite;
    public Sprite freezeSprite;

    private void Start()
    {
        if (type == PowerUpType.ChangeColor)
        {
            spriteRenderer.sprite = changeColorSprite;
        }
        else if (type == PowerUpType.Freeze)
        {
            spriteRenderer.sprite = freezeSprite;
        }
    }
}