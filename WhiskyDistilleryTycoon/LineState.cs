using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LineState : IGameState
{
    public static LineState instance;
    public IGameState RunState(StateMachine _statemachine)
    {
        LineUpContainer.instance.completelistbutton.active = false;
        LineUpContainer.instance.cancellistbutton.active = false;
        foreach(LineRenderer l in LineUpContainer.instance.arrayofinactiveLinerenderers)
        {
            l.enabled = true;
        }
        DisplayConnections();

        if (_statemachine.onLineToolClick)
        {
            _statemachine.onLineToolClick = false;
            return _statemachine.normalState;
        }
        if (_statemachine.onBlueToolClick)
        {
            _statemachine.ActivateBlueTool();
            return _statemachine.blueState;
        }
        if (_statemachine.onBuildingMenueClick)
        {
            _statemachine.onBuildingMenueClick = false;
            return _statemachine.normalState;
        }
        if (_statemachine.onStartBuildingClick)
        {
            _statemachine.onStartBuildingClick = false;
            return _statemachine.chainState;
        }
        return _statemachine.lineState;
    }

    private void DisplayConnections()
    {
        LineUpContainer.instance.deletebuton.SetActive(true);
        Ray ray = LineUpContainer.instance.Cam.ScreenPointToRay(Input.mousePosition);
        LineUpContainer.instance.redlineconnectingmouseandlastaktuellelinemember.enabled = false;
        LineUpContainer.instance.greenlineconnectingaktuelleline.enabled = false;
    }

}
