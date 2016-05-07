using UnityEngine;
using scMessage;
using System.Collections;

public class roboScript : MonoBehaviour {

    int screenWidth, screenHeight;
    int desiredX, desiredY;
    public GameObject arm1, arm2, arm3, brush, paint;
    float[][] dhParams = new float[5][];
    Matrix4x4 T01, T12, T23;
    int originX, originY;
    public loginScript lgScript;
    public Camera ourCam;
    public serverTCP ourServ;
    bool isDelayed;

    void updateRobot()
    {
        calculateT(1);
        calculateT(2);
        calculateT(3);
        calculateArm(1);
        calculateArm(2);
        calculateArm(3);
    }

    void updateFlags()
    {
        if (ourServ.message == '0')
        {
            ourServ.moveLeft = true;
        }
        else if (ourServ.message == '1')
        {
            ourServ.moveRight = true;
        }
        else if (ourServ.message == '2')
        {
            ourServ.arm2clock = true;
        }
        else if (ourServ.message == '3')
        {
            ourServ.arm2count = true;
        }
        else if (ourServ.message == '4')
        {
            ourServ.arm3clock = true;
        }
        else if (ourServ.message == '5')
        {
            ourServ.arm3count = true;
        }
        else if (ourServ.message == '6')
        {
            ourServ.yPlus = true;
        }
        else if (ourServ.message == '7')
        {
            ourServ.yMinus = true;
        }
        else if (ourServ.message == '8')
        {
            ourServ.paint = true;
        }
    }

    void Update()
    {
        if (ourServ != null)
        {
            if (ourServ.isInvoke == true)
            {
                Invoke("updateFlags", 2);
                ourServ.isInvoke = false;
            }
                

            if (ourServ.moveLeft)
            {
                moveLeft();
                ourServ.moveLeft = false;
            } else if (ourServ.moveRight)
            {
                moveRight();
                ourServ.moveRight = false;
            }
            else if (ourServ.arm2clock)
            {
                arm2clock();
                ourServ.arm2clock = false;
            }
            else if (ourServ.arm2count)
            {
                arm2counterc();
                ourServ.arm2count = false;
            }
            else if (ourServ.arm3clock)
            {
                arm3clock();
                ourServ.arm3clock = false;
            }
            else if (ourServ.arm3count)
            {
                arm3counterc();
                ourServ.arm3count = false;
            }
            else if (ourServ.yPlus)
            {
                yPlus();
                ourServ.yPlus = false;
            }
            else if (ourServ.yMinus)
            {
                yMinus();
                ourServ.yMinus = false;
            }
            else if (ourServ.paint)
            {
                paintPoint();
                ourServ.paint = false;
            }
        }
    }

	// Use this for initialization
	void Start () {

        screenWidth = Screen.width;
        screenHeight = Screen.height;
        isDelayed = false;

        originX = (int)Camera.main.WorldToScreenPoint(arm1.transform.position).x;
        originY = (int)Camera.main.WorldToScreenPoint(arm1.transform.position).y;

        desiredX = 0;
        desiredY = 0;

        //[n][] 0 is origin, 1 is base of arm1, 2 arm2, 3 arm3, 4 is brush location
        //[][n] 0 is alpha i-1, 1 is a i-1, 2 is d i, 3 is theta i
        dhParams[0] = new float[4] { 0, 0, 0, 0 };
        dhParams[1] = new float[4] { 0, 175, 0, 0 };
        dhParams[2] = new float[4] { 0, 150, 0, 90 };
        dhParams[3] = new float[4] { 0, 100, 0, 0 };
        dhParams[4] = new float[4] { 0, 75, 0, 0 };
        
        //Our 3 transformation matrix's
        T01 = new Matrix4x4();
        T12 = new Matrix4x4();
        T23 = new Matrix4x4();

        desiredX = (int)Camera.main.WorldToScreenPoint(brush.transform.position).x - (int)Camera.main.WorldToScreenPoint(arm2.transform.position).x;
        desiredY = (int)Camera.main.WorldToScreenPoint(brush.transform.position).y - (int)Camera.main.WorldToScreenPoint(arm2.transform.position).y;

        updateRobot();
    }
	
    void calculateT(int i)
    {
        Matrix4x4 temp = new Matrix4x4();
        float alpha, a, d, theta;
        alpha = dhParams[i][0] * Mathf.Deg2Rad;
        a = dhParams[i][1];
        d = dhParams[i][2];
        theta = dhParams[i][3] * Mathf.Deg2Rad;

        temp.m00 = (Mathf.Cos(theta));
        temp.m01 = -(Mathf.Cos(alpha) * Mathf.Sin(theta));
        temp.m02 = Mathf.Sin(alpha) * Mathf.Sin(theta);
        temp.m03 = a * Mathf.Cos(theta);
        temp.m10 = Mathf.Sin(theta);
        temp.m11 = ((Mathf.Cos(theta) * Mathf.Cos(alpha)));
        temp.m12 = -(Mathf.Sin(alpha) * Mathf.Cos(theta));
        temp.m13 = a * Mathf.Sin(theta);
        temp.m20 = 0;
        temp.m21 = Mathf.Sin(alpha);
        temp.m22 = Mathf.Cos(alpha);
        temp.m23 = d;
        temp.m30 = 0;
        temp.m31 = 0;
        temp.m32 = 0;
        temp.m33 = 1;

        if (i == 1)
            T01 = temp;
        else if (i == 2)
            T12 = temp;
        else
            T23 = temp;
    }

    void calculateArm(int i)
    {

        if(i == 1)
        {
            Vector3 temp = new Vector3(T01.m03 + originX, T01.m13 + originY, T01.m23);
            temp = ourCam.ScreenToWorldPoint(temp);
            temp.z = 0;
            arm1.transform.position = temp;
		} else if(i == 2)
		{
            Matrix4x4 T02 = T01 * T12;
			Vector3 temp = new Vector3(T02.m03 + originX, T02.m13 + originY, T02.m23);
			temp = Camera.main.ScreenToWorldPoint(temp);
			temp.z = 0;
			arm2.transform.position = temp;
		} else
        {
            Matrix4x4 T03 = T01 * T12 * T23;
            Vector3 temp = new Vector3(T03.m03 + originX, T03.m13 + originY, T03.m23);
            temp = Camera.main.ScreenToWorldPoint(temp);
            temp.z = 0;
            arm3.transform.position = temp;
        }

    }
    
    void inverseCalc()
    {
        float arg = (Mathf.Pow(desiredX, 2) + Mathf.Pow(desiredY, 2) - Mathf.Pow(100, 2) - Mathf.Pow(75, 2)) / (2 * 100 * 75); 
        float theta2 = Mathf.Acos(arg);


        float k1 = 100 + 75 * Mathf.Cos(theta2);
        float k2 = 75 * Mathf.Sin(theta2);
        float r = Mathf.Sqrt(Mathf.Pow(k1, 2) + Mathf.Pow(k2, 2));
        float gamma = Mathf.Atan2(k2, k1);
        k1 = r * Mathf.Cos(gamma);
        k2 = r * Mathf.Sin(gamma);
        float theta1 = Mathf.Atan2(desiredY, desiredX) - Mathf.Atan2(k2, k1);


        dhParams[2] = new float[4] { 0, 150, 0, 90 };
        dhParams[3] = new float[4] { 0, 100, 0,  -90 + theta1 * Mathf.Rad2Deg};
        dhParams[4] = new float[4] { 0, 75, 0, 0};

        arm2.transform.rotation = new Quaternion(0, 0, 0, 0);        
        arm2.transform.Rotate(new Vector3(0, 0, 1), -90);
        arm2.transform.Rotate(new Vector3(0, 0, 1), theta1 * Mathf.Rad2Deg);

        arm3.transform.rotation = new Quaternion(0, 0, 0, 0);
        arm3.transform.Rotate(new Vector3(0, 0, 1), -90);
        arm3.transform.Rotate(new Vector3(0, 0, 1), theta1 * Mathf.Rad2Deg);
        arm3.transform.Rotate(new Vector3(0, 0, 1), theta2 * Mathf.Rad2Deg);
        

    }

    public void moveLeft()
    {
        if (dhParams[1][1] > 0)
        {
            dhParams[1][1] -= 1;
            updateRobot();
        }

        if (lgScript.connectedToServer)
        {
            lgScript.sendServerMessage(new message("0"));
        }
    } 

    public void moveRight()
    {
        if (dhParams[1][1] < 335)
        {
            dhParams[1][1] += 1;
            updateRobot();
        }

        if (lgScript.connectedToServer)
        {
            lgScript.sendServerMessage(new message("1"));
        }
    }

    public void arm2clock()
    {
        dhParams[3][3] -= 1;
        updateRobot();

        arm2.transform.Rotate(new Vector3(0, 0, 1), -1);
        arm3.transform.Rotate(new Vector3(0, 0, 1), -1);

        desiredX = (int)Camera.main.WorldToScreenPoint(brush.transform.position).x - (int)Camera.main.WorldToScreenPoint(arm2.transform.position).x;
        desiredY = (int)Camera.main.WorldToScreenPoint(brush.transform.position).y - (int)Camera.main.WorldToScreenPoint(arm2.transform.position).y;

        if (lgScript.connectedToServer)
        {
            lgScript.sendServerMessage(new message("2"));
        }
    }

    public void arm2counterc()
    {
        dhParams[3][3] += 1;
        updateRobot();

        arm2.transform.Rotate(new Vector3(0, 0, 1), 1);
        arm3.transform.Rotate(new Vector3(0, 0, 1), 1);

        desiredX = (int)Camera.main.WorldToScreenPoint(brush.transform.position).x - (int)Camera.main.WorldToScreenPoint(arm2.transform.position).x;
        desiredY = (int)Camera.main.WorldToScreenPoint(brush.transform.position).y - (int)Camera.main.WorldToScreenPoint(arm2.transform.position).y;

        if (lgScript.connectedToServer)
        {
            lgScript.sendServerMessage(new message("3"));
        }
    }

    public void arm3clock()
    {
        dhParams[4][3] -= 1;
        updateRobot();

        arm3.transform.Rotate(new Vector3(0, 0, 1), -1);

        desiredX = (int)Camera.main.WorldToScreenPoint(brush.transform.position).x - (int)Camera.main.WorldToScreenPoint(arm2.transform.position).x;
        desiredY = (int)Camera.main.WorldToScreenPoint(brush.transform.position).y - (int)Camera.main.WorldToScreenPoint(arm2.transform.position).y;

        if (lgScript.connectedToServer)
        {
            lgScript.sendServerMessage(new message("4"));
        }
    }

    public void arm3counterc()
    {
        dhParams[4][3] += 1;
        updateRobot();

        arm3.transform.Rotate(new Vector3(0, 0, 1), 1);

        desiredX = (int)Camera.main.WorldToScreenPoint(brush.transform.position).x - (int)Camera.main.WorldToScreenPoint(arm2.transform.position).x;
        desiredY = (int)Camera.main.WorldToScreenPoint(brush.transform.position).y - (int)Camera.main.WorldToScreenPoint(arm2.transform.position).y;

        if (lgScript.connectedToServer)
        {
            lgScript.sendServerMessage(new message("5"));
        }
    }

    public void yPlus()
    {
        desiredY += 5;
        if (((Mathf.Pow(desiredX, 2) + Mathf.Pow(desiredY, 2)) <= Mathf.Pow((100 + 75), 2)) && ((Mathf.Pow(desiredX, 2) + Mathf.Pow(desiredY, 2)) >= Mathf.Pow((100 - 75), 2)))
        {
            inverseCalc();
            updateRobot();
        }
        else desiredY -= 5;

        if (lgScript.connectedToServer)
        {
            lgScript.sendServerMessage(new message("6"));
        }
    }

    public void yMinus()
    {
        desiredY -= 5;
        if (((Mathf.Pow(desiredX, 2) + Mathf.Pow(desiredY, 2)) <= Mathf.Pow((100 + 75), 2)) && ((Mathf.Pow(desiredX, 2) + Mathf.Pow(desiredY, 2)) >= Mathf.Pow((100 - 75), 2)))
        {
            inverseCalc();
            updateRobot();
        }
        else desiredY += 5;

        if (lgScript.connectedToServer)
        {
            lgScript.sendServerMessage(new message("7"));
        }
    }

    public void paintPoint()
    {
        GameObject temp = GameObject.Instantiate<GameObject>(paint);
        Vector3 XYTemp = new Vector3();

        XYTemp.x = desiredX + Camera.main.WorldToScreenPoint(arm2.transform.position).x;
        XYTemp.y = desiredY + Camera.main.WorldToScreenPoint(arm2.transform.position).y;

        XYTemp = Camera.main.ScreenToWorldPoint(XYTemp);

        XYTemp.z = 0;

        temp.transform.position = XYTemp;

        if (lgScript.connectedToServer)
        {
            lgScript.sendServerMessage(new message("8"));
        }
    }

    void OnGUI()
    {
        if (!lgScript.isServer())
        {
            GUI.Box(new Rect(screenWidth - 110, 10, 100, 195), "Joint Controls\nArm 1\n\n\n\nArm 2\n\n\n\nArm 3");

            if (GUI.Button(new Rect(screenWidth - 100, 45, 80, 20), "Left"))
            {
                moveLeft();
            }

            if (GUI.Button(new Rect(screenWidth - 100, 65, 80, 20), "Right"))
            {
                moveRight();
            }

            if (GUI.Button(new Rect(screenWidth - 100, 105, 80, 20), "Clockwise"))
            {
                arm2clock();
            }

            if (GUI.Button(new Rect(screenWidth - 100, 125, 80, 20), "CounterC"))
            {
                arm2counterc();
            }

            if (GUI.Button(new Rect(screenWidth - 100, 165, 80, 20), "Clockwise"))
            {
                arm3clock();
            }

            if (GUI.Button(new Rect(screenWidth - 100, 185, 80, 20), "CounterC"))
            {
                arm3counterc();
            }

            //Project 2
            GUI.Box(new Rect(screenWidth - 110, 215, 100, 175), "World Controls\nJoint 1\n\n\n\nBrush");

            //Joint1
            if (GUI.Button(new Rect(screenWidth - 100, 250, 35, 20), "X+"))
            {
                moveRight();
            }

            if (GUI.Button(new Rect(screenWidth - 55, 250, 35, 20), "Y+"))
            {
                yPlus();
            }

            if (GUI.Button(new Rect(screenWidth - 100, 270, 35, 20), "X-"))
            {
                moveLeft();
            }

            if (GUI.Button(new Rect(screenWidth - 55, 270, 35, 20), "Y-"))
            {
                yMinus();
            }

            //Paint

            if (GUI.Button(new Rect(screenWidth - 100, 310, 80, 20), "Paint"))
            {
                paintPoint();
            }
        }

        if (lgScript.connectedToServer)
        {
            if (isDelayed)
            {
                GUI.Box(new Rect(0, 10, 100, 70), "Delayed Mode");

                if (GUI.Button(new Rect(0, 50, 100, 20), "Change Mode"))
                {
                    isDelayed = false;
                    lgScript.sendServerMessage(new message("9"));
                }
            }
            else
            {
                GUI.Box(new Rect(0, 10, 100, 70), "Normal Mode");

                if (GUI.Button(new Rect(0, 50, 100, 20), "Change Mode"))
                {
                    isDelayed = true;
                    lgScript.sendServerMessage(new message("9"));
                }
            }
        }

    }
}
