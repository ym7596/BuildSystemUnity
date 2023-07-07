using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMyObject
{
	bool CanPlaced { get; set; }
	bool Placed { get; set; }

    void SetName(string name);
	void SetUniqueID(int id);
    void SetPlaced(bool isOn);
    void SetRotate(int value);
    void SetMove(Vector3 pos);
    void SetBaseColor(Color color);
}
