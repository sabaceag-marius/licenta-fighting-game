[System.Serializable]

public struct RawInput
{
    public bool IsConfirmed;
    
    public ushort FrameId;

    public sbyte LeftStickX;
    
    public sbyte LeftStickY;

    public byte RightStick; //0000LeftRightUpDown

    public byte Buttons; // 000GrabDodgeSpecialAttackJump

    public bool Jumped => (Buttons & (1 << 0)) != 0;

    public bool Attacked => (Buttons & (1 << 1)) != 0;

    public bool SpecialAttacked => (Buttons & (1 << 2)) != 0;
    
    public bool Dodged => (Buttons & (1 << 3)) != 0;

    public bool Grabbed => (Buttons & (1 << 4)) != 0;

    public bool RightStickDown => (RightStick & (1 << 0)) != 0;
    public bool RightStickUp => (RightStick & (1 << 1)) != 0;
    public bool RightStickRight => (RightStick & (1 << 2)) != 0;
    public bool RightStickLeft => (RightStick & (1 << 3)) != 0;
    
    public bool IsEmpty => LeftStickX == 0 && LeftStickY == 0 && Buttons == 0 && RightStick == 0;
}