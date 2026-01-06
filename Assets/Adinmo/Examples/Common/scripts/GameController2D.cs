using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Adinmo.Examples
{
    public class GameController2D : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField]
        GameObject BackgroundLayer;
        [SerializeField]
        GameObject ForegroundLayer;
        [SerializeField]
        SpriteRenderer characterSprite;
        [SerializeField]
        Sprite[] characterFrames;
        [SerializeField]
        GameObject pauseMenu;
        bool leftDown = false;
        bool rightDown = false;

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || leftDown)
            {
                MoveLeft();
            }
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || rightDown)
            {
                MoveRight();
            }
        }

        public void MoveLeft()
        {
            if (ForegroundLayer.transform.localPosition.x < -10)
            {
                BackgroundLayer.transform.Translate(Time.deltaTime * 1.5f, 0, 0);
                ForegroundLayer.transform.Translate(Time.deltaTime * 2.25f, 0, 0);
                characterSprite.flipX = false;
                int frame = (int)Math.Abs((ForegroundLayer.transform.localPosition.x * 3) % 2);
                characterSprite.sprite = characterFrames[frame];
            }
        }
        public void MoveRight()
        {
            if (ForegroundLayer.transform.localPosition.x > -56)
            {
                BackgroundLayer.transform.Translate(-Time.deltaTime * 1.5f, 0, 0);
                ForegroundLayer.transform.Translate(-Time.deltaTime * 2.25f, 0, 0);
                characterSprite.flipX = true;
                int frame = (int)Math.Abs((ForegroundLayer.transform.localPosition.x * 3) % 2);
                characterSprite.sprite = characterFrames[frame];
            }
        }

        public void OnLeftDown()
        {
            leftDown = true;
        }
        public void OnLeftUp()
        {
            leftDown = false;
        }


        public void OnRightDown()
        {
            rightDown = true;
        }
        public void OnRightUp()
        {
            rightDown = false;
        }
        public void PressPause()
        {
            pauseMenu.SetActive(true);
        }

        public void PressResume()
        {
            pauseMenu.SetActive(false);
        }
    }
}