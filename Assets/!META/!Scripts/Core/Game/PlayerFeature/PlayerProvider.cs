using BitterECS.Integration;
using UnityEngine;

[RequireComponent(typeof(MovingComponentProvider))]
public class PlayerProvider : MonoProvider<PlayerPresenter>
{ }
