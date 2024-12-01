using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    public GameObject heart1;
    public GameObject heart2;
    public GameObject heart3;
    public GameObject heart4;
    public GameObject heart5;

    public Sprite healthFull;
    public Sprite healthEmpty;

    private Image[] hearts;
    private PlayerController playerController;

    void Start()
    {
        hearts = new Image[5];

        hearts[0] = heart1.GetComponent<Image>();
        hearts[1] = heart2.GetComponent<Image>();
        hearts[2] = heart3.GetComponent<Image>();
        hearts[3] = heart4.GetComponent<Image>();
        hearts[4] = heart5.GetComponent<Image>();

        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    void Update()
    {
        int healthRemain = playerController.health;
        for (int i = 0; i < healthRemain; ++i)
        {
            hearts[i].sprite = healthFull;
        }

        for (int i = healthRemain; i < 5; ++i)
        {
            hearts[i].sprite = healthEmpty;
        }
    }
}
