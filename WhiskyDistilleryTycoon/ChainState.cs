using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class ChainState : IGameState
{
    public static ChainState instance;
    public Line AktuelleKette;

    public IGameState RunState(StateMachine _statemachine)
    {
        DisplayConnections();
        LineUpContainer.instance.cancellistbutton.active = true;
        if (LineUpContainer.instance.aktuellekette.line.Count > 0)
        {
            if (LineUpContainer.instance.aktuellekette.line[LineUpContainer.instance.aktuellekette.line.Count - 1].ID == 6)
            {
                LineUpContainer.instance.giveoptionstocompleteline = true;
                LineUpContainer.instance.completelistbutton.active = true;
            }
            else
            {
                LineUpContainer.instance.completelistbutton.active = false;
            }
        }
        if (LineUpContainer.instance.currentlineCompleted)
        {
            LineUpContainer.instance.currentlineCompleted = false;
            LineUpContainer.instance.aktuellerInaktiverLinerenderer.positionCount = LineUpContainer.instance.aktuellekette.line.Count;
            LineUpContainer.instance.aktuellerInaktiverLinerenderer.SetPositions(LineUpContainer.instance.aktuelleListePositions);
            LineUpContainer.instance.aktuellerInaktiverLinerenderer.SetPosition(LineUpContainer.instance.aktuellekette.line.Count - 1, LineUpContainer.instance.aktuellekette.line[LineUpContainer.instance.aktuellekette.line.Count - 1].transform.position + Vector3.up);
            LineUpContainer.instance.aktuellerInaktiverLinerenderer = LineRenderer.Instantiate(LineUpContainer.instance.aktuellerInaktiverLinerenderer);
            LineUpContainer.instance.arrayofinactiveLinerenderers.Add(LineUpContainer.instance.aktuellerInaktiverLinerenderer);
            StateMachine.instance.OnChainFinish = true;
        }
        if (_statemachine.onLineToolClick)
        {
            LineUpContainer.instance.redlineconnectingmouseandlastaktuellelinemember.enabled = false;
            LineUpContainer.instance.greenlineconnectingaktuelleline.enabled = false;
            LineUpContainer.instance.aktuellekette.line.Clear();
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
        if (_statemachine.OnChainFinish)
        {
            _statemachine.OnChainFinish = false;
            return _statemachine.lineState;
        }

        return _statemachine.chainState;
    }
    private void DisplayConnections()
    {
        LineUpContainer.instance.redlineconnectingmouseandlastaktuellelinemember.enabled = true;
        LineUpContainer.instance.greenlineconnectingaktuelleline.enabled = true;
        if (LineUpContainer.instance.aktuellekette.line.Count >= 0)
        {
            LineUpContainer.instance.greenlineconnectingaktuelleline.positionCount = LineUpContainer.instance.aktuellekette.line.Count;
        }
        Ray ray = LineUpContainer.instance.Cam.ScreenPointToRay(Input.mousePosition);
        if (LineUpContainer.instance.aktuellekette.line.Count >= 1 && Physics.Raycast(ray, out LineUpContainer.instance.raycasthit, 1000f))
        {
            LineUpContainer.instance.redlineconnectingmouseandlastaktuellelinemember.SetPosition(0, LineUpContainer.instance.aktuellekette.line[LineUpContainer.instance.aktuellekette.line.Count - 1].GetComponent<PlaceableObject>().centerpoint.position + Vector3.up);
            LineUpContainer.instance.redlineconnectingmouseandlastaktuellelinemember.SetPosition(1, LineUpContainer.instance.raycasthit.point+ Vector3.up);
            for (int i = 0; i < LineUpContainer.instance.aktuellekette.line.Count; i++)
            {
                LineUpContainer.instance.greenlineconnectingaktuelleline.SetPosition(i, LineUpContainer.instance.aktuellekette.line[i].GetComponent<PlaceableObject>().centerpoint.position + Vector3.up);
                LineUpContainer.instance.aktuelleListePositions[i] = LineUpContainer.instance.aktuellekette.line[i].GetComponent<PlaceableObject>().centerpoint.position+ Vector3.up;
            }
        }
    }
}