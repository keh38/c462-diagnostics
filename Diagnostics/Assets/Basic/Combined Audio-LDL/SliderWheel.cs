using System.Collections;
using UnityEngine;

public class SliderWheel : MonoBehaviour
{
    [SerializeField] private CombinedSliderAnimator[] _sliders; // 5 sliders, assigned in inspector
    [SerializeField] private float _animationDuration = 0.4f;

    // Each slot has a position and a scale
    // Slot 0 = top (smallest), Slot 2 = active (largest), Slot 4 = below screen (hidden)
    private readonly Vector2[] _slotPositions = new Vector2[]
    {
        new Vector2(0,  900),  // slot -1 - above screen, hidden
        new Vector2(0,  700),  // slot 0 - top
        new Vector2(0,  500),  // slot 1
        new Vector2(0,   -50),  // slot 2 - active
        new Vector2(0, -550),  // slot 3
        new Vector2(0, -850),  // slot 4 - below screen, hidden
    };

    private readonly Vector2[] _slotScales = new Vector2[]
    {
        new Vector2(0.25f,  0.25f),  // slot -1
        new Vector2(0.5f,   0.5f),  // slot 0
        new Vector2(0.75f,   0.75f),  // slot 1
        new Vector2(1.0f,   1.0f),  // slot 2 - active
        new Vector2(0.75f,   0.75f),  // slot 3
        new Vector2(0.5f,  0.5f),  // slot 4
    };

    // _slotAssignment[slotIndex] = which slider is currently in that slot
    private int[] _slotAssignment;
    private int _advanceCount = 0;

    private void Start()
    {
        _slotAssignment = new int[] { 0, 1, 2, 3, 4 };

        // Snap all sliders to their initial positions immediately
        for (int slot = 0; slot < 5; slot++)
        {
            _sliders[_slotAssignment[slot]].SnapTo(_slotPositions[slot + 1], _slotScales[slot + 1]);

            // Hide the two slots that represent not-yet-completed trials
            if (slot < 2)
                _sliders[_slotAssignment[slot]].SetVisible(false);
        }
    }

    public void Advance()
    {
        StartCoroutine(AdvanceCoroutine());
    }

    private IEnumerator AdvanceCoroutine()
    {
        _advanceCount++;

        // Reveal the appropriate slot the first two times
        if (_advanceCount == 2)
            _sliders[0].SetVisible(true);
        else if (_advanceCount == 3)
            _sliders[1].SetVisible(true);

        // Now tell every slider where it's going
        for (int slot = 0; slot < 5; slot++)
            _sliders[_slotAssignment[slot]].AnimateTo(
                _slotPositions[slot],
                _slotScales[slot],
                _animationDuration);

        // Wait for animation to complete before allowing another Advance
        yield return new WaitForSeconds(_animationDuration);

        // The slider currently in slot 0 is about to exit off the top.
        // Before animating, silently move it to slot 4 (below screen)
        // so it's ready to scroll up as the new incoming slider.
        int exitingSlider = _slotAssignment[0];
        _sliders[exitingSlider].SnapTo(_slotPositions[5], _slotScales[5]);

        // Rotate slot assignments: slot[n] gets what was in slot[n+1]
        // The exiting slider takes slot 4 (the incoming position)
        for (int slot = 0; slot < 4; slot++)
            _slotAssignment[slot] = _slotAssignment[slot + 1];
        _slotAssignment[4] = exitingSlider;

    }
}
