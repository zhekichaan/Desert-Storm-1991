using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BountyDetails_", menuName = "Scriptable Objects/Enemy/BountyDetails")]
public class BountyDetailsSO : ScriptableObject
{
    #region Header BOUNTY CARD
    [Space(10)]
    [Header("BOUNTY CARD")]
    #endregion

    #region Tooltip
    [Tooltip("Display name shown on the bounty card (can differ from the enemy's internal name)")]
    #endregion
    public string bountyName;

    #region Tooltip
    [Tooltip("Portrait sprite shown on the bounty card")]
    #endregion
    public Sprite portraitSprite;

    // #region Tooltip
    // [Tooltip("The location/district associated with this target")]
    // #endregion
    // public string location;

    #region Tooltip
    [Tooltip("List of crimes this target is wanted for")]
    #endregion
    public string crimes;

    #region Tooltip
    [Tooltip("Flavor text description shown on the bounty card")]
    #endregion
    [TextArea(3, 10)]
    public string description;

    // #region Tooltip
    // [Tooltip("Small badge icons shown top-right of the card (e.g. threat level, weapon type)")]
    // #endregion
    // public List<Sprite> badgeIcons;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(bountyName), bountyName);
        HelperUtilities.ValidateCheckEmptyString(this, nameof(crimes), crimes);
        HelperUtilities.ValidateCheckNullValue(this, nameof(portraitSprite), portraitSprite);
        HelperUtilities.ValidateCheckEmptyString(this, nameof(description), description);
    }
#endif
    #endregion
}