using UnityEngine;

namespace CompoundBox
{
    public interface IBoardVisualTheme
    {
        Sprite FloorSprite { get; }
        Sprite WallSprite { get; }
        Sprite GoalSprite { get; }
        Sprite ExitSprite { get; }
        Sprite PortalSprite { get; }
        Sprite PlayerSprite { get; }
        Sprite MatterSprite { get; }
    }
}
