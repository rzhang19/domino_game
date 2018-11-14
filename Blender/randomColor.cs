using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class randomColor : MonoBehaviour {
	// Use this for initialization
	void Start () {
        Renderer rend = GetComponent<Renderer>();
        Color randomColor = RandomColor();

        rend.material.shader = Shader.Find("_Color");
        rend.material.SetColor("_Color", randomColor);

        rend.material.shader = Shader.Find("Specular");
        rend.material.SetColor("_SpecColor", randomColor);
	}

    Color RandomColor()
    {
        return new Color(Random.value, Random.value, Random.value);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
