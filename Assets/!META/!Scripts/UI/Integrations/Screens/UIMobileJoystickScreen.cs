using System;
using UnityEngine;

[RequireComponent(typeof(Joystick))]
public class UIMobileJoystickScreen : UIScreen
{
    public override void Open()
    {
        MobileInputSystem.Joystick ??= GetComponent<Joystick>();

        base.Open();
    }
}

