/*--------------------------------------------------------------------------------------------------------------------------------------------------------------------

           _________   __________    _________    __________
                   /  |          |  |         |  |          |
                  /   |          |  |         |  |          |
                 /    |          |  |         |  |          |
                /     |          |  |         |  |          |
               /      |          |  |_________|  |          |
              /       |          |  | \          |          |
             /        |          |  |  \         |          |
            /         |          |  |   \        |          |
           /_______   |__________|  |    \_____  |__________|         ...Assets, Prototypes & Co.


        
           
         Free to use for commercial projects & personal projects!
           
        

           
        
        
        
     If you want to support my work, feel free to support me via paypal:

                        paypal.me/ZoroArts

------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
/*
 
         Documentation:

This gamemechanic is just a prototype and a possible way to let a gameobject follow your path, its not finished yet but their will be updates regularely!


What do I need to setup this project?

    It`s pretty simple and customizable. All you need is:


      -This script or a modified version

      -A line Renderer (to show the path. I attached it on an other & independent gameobject)

      -A player with a rigidbody (its not important to copy mine, you can choose them your self) and a box collider 
      (to make sure that the player cant fall through the ground)

      -A prefab that contains an empty gameObject (That works as a waypoint & is instantiated by this script)



How does the mechanic works?

    This script is really simple all the steps are explained really accurately in the code step by step

*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody rb;
    public LineRenderer lr;

    [Header("Customizable Values")]
    public float timeForNextRay = 0.05f;

    //Not customizable:
    private float timer = 0;
    private int currentWayPoint = 0;
    private int wayIndex;
    private bool move;
    private bool touchStartedOnPlayer;
    public List<GameObject> wayPoints;

    [Header("Prefabs")]
    public GameObject wayPoint;

    void Start()
    {
        //The Rigidbody gets referenced and the line renderer is referenced in the inspector, because it is not on this Object
        rb = GetComponent<Rigidbody>();

        //The Line Renderer is disabled at start and gets enabled when you draw the path
        lr.enabled = false;

        //The wayindex is 1 because there is no waypoint at the start. The waypoint get instantiated while drawing the path
        wayIndex = 1;

        // move is false, to make sure that you are not moving at start, but you move after you drew the path
        move = false;
        touchStartedOnPlayer = false;
    }


    public void OnMouseDown()
    {
        //This gets triggered when the mouse touches the Object of this script (the player Collider)
        //Line Renderer is enabled now to see the line it is creating
        lr.enabled = true;

        touchStartedOnPlayer = true;

        //The position count is 1 at this moment (The only & FIRST position is the player position, 
        //to make sure that the path starts on the players Position)
        lr.positionCount = 1;
        lr.SetPosition(0, transform.position);

    }
    void Update()
    {
        //This is the main Code:
        //It basically instantiates a waypoint every X seconds (You can change the time between spawning by changing 
        //the "timeForNextRay" value, a higher value is better for the performance but also very inaccurate. The player will
        //also move faster with less waypoints to pass. 
        //A smaller value is really accurate and the player will move towards its destination smoothly.

        if (Input.GetMouseButton(0) && timer > timeForNextRay && touchStartedOnPlayer)
        {
            //This code gets triggered when you hold the left mouse Button down & the timer allows you to create the next Waypoint


            //Its really hard to get the mouse position in world coordinates. I used the "Camera.main.ScreenToWorldPoint" to 
            //transform the mouse Vector3 from screen space to world Space:
            //Unity Documentation link for "Camera.Main.ScreenToWorldPoint":   https://docs.unity3d.com/ScriptReference/Camera.ScreenToWorldPoint.html
            Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 100));


            //The next step was to subtract the Camera Position from the mouse Position Vector3 to get the best result & 
            //place the waypoints as accurate as they could be. Now it doesnt matter where the Camera is or how it is rotated,
            //the result will be accurate
            Vector3 direction = worldMousePos - Camera.main.transform.position;



            //The next if Statement is the Raycast part of the script.
            //A raycast is basically just a line with a limited (in my case), or infinte range. It gets used together with 
            //collision detection often. A common use case is FPS (First Person Shooter) Shooting. Many popular games like CSGO,
            //Overwatch or others use Raycasts instead of bullets, because Raycasts are way more accurate than bullets.
            //You will get better results with Raycasts.
            //Unity Documentation link for "Physics.Raycast": https://docs.unity3d.com/ScriptReference/Physics.Raycast.html

            RaycastHit hit;
            if(Physics.Raycast(Camera.main.transform.position, direction, out hit, 100f))
            {
                //This is just for debugging. The Raycasts are visible now through the Scene View
                Debug.DrawLine(Camera.main.transform.position, hit.point, Color.green, 1f);

                //In my case, I send a Raycast every X seconds from the Main Camera Position to see where the mousePosition
                //would be on the ground platform. With the collision Point of the Raycast, the "hit", I can easily instantiate the
                //Waypoint prefab exactly on the mouse Position that you see through the Game View. 
                //(Important!!!: Use the Y Position of the player Object to avoid the object flying through the scene or getting stucked in the ground)
                //I'm working on a better solution at the moment, so you will be able to pass ramps in the future too.
                GameObject newWaypoint = Instantiate(wayPoint, new Vector3(hit.point.x, transform.position.y, hit.point.z), Quaternion.identity);

                //The next step is to save your currently placed waypoint to store them while instantiating 
                //the next one (I used a list of Gameobjects to save the waypoints). 
                //Your path just consists of many invisible waypoints, which are connected with a line through the Line Renderer
                //To add the current waypoint to your list, use this: "Listname".Add("Waypoint Object Name") 
                //Unity-Learn Link for "Lists": https://learn.unity.com/tutorial/lists-and-dictionaries
                wayPoints.Add(newWaypoint);

                //After adding the point to your list, you have to set the next Point of the Linerenderer 
                //to make sure, that it continues to draw the line
                //Before you do that, you have to increase the Position Count of the Line Renderer.
                //Then you can add a next Position to the Line Renderer with 
                //LineRenderer.SetPosition(Noumber of the position, Your Position as Vector3)
                //Unity Documentation Link of the "Line Renderer": https://docs.unity3d.com/Manual/class-LineRenderer.html
                lr.positionCount = wayIndex + 1;
                lr.SetPosition(wayIndex, newWaypoint.transform.position);

                //Next, you just have to set the timer value to 0. It is Important 
                //to reset the timer to 0, because the "timeForNextRay" stays at the same value, all
                //the time, so if your timer value is higher than the "timeForNextRay" the code does not work anymore.
                timer = 0;

                //You also have to increase the "wayIndex" which is important for the "move" code
                //You will use the "WayIndex" to iterate through your waypoint List 
                //and find the next Point your Player has to move & Rotate towards
                //The "wayIndex is always the last waypoint of your list
                wayIndex++;
            }
        }

        //The timer will be increased by adding the Time.deltaTime Value every second (The timer value just works as a second Timer)
        timer += Time.deltaTime;

        // If you stop to hold the left mouse Button down, the bool "move" will become true and the player starts to move:
        if (Input.GetMouseButtonUp(0))
        {
            touchStartedOnPlayer = false;
            move = true;
        }

        if (move)
        {
            //Transform.LookAt rotetes the transform of your player, that his forward direction faces the next waypoint
            //The currentWayPoint is always the next waypoint to move & rotete towards
            //You basically take the next waypoint of your waypoint List & move & rotate towards it
            transform.LookAt(wayPoints[currentWayPoint].transform);

            //Rb.MovePosition moves the !Kinematic! Rigidbody towards the next waypoint position
            //You can choose other methods & make the Rigidbody not Kinematic, to allow 
            //collisions while moving, to avoid that the player walks through walls
            rb.MovePosition(wayPoints[currentWayPoint].transform.position);

            //When the Position of the player is equal to see position of the next waypoint, 
            //the currentWayPoint value gets increased by one (to take the next waypoint from the list)
            if (transform.position == wayPoints[currentWayPoint].transform.position) currentWayPoint++;

            //This code gets triggered when the player arrived the end of the path (its destination)
            if(currentWayPoint == wayPoints.Count)
            {
                //move is false to change the mode to the draw mode
                move = false;

                //this foreach loop triggers the "Destroy" function for each of the waypoint elements in your list
                foreach (var item in wayPoints) Destroy(item);

                //Waypoints.Clear clears the waypoint list, to make sure that the next waypoint will be the second one in the list
                wayPoints.Clear();

                //WayIndex  will be resetted to 1
                wayIndex = 1;

                //The currentWayPoint is 0 because an array or a list always starts with 0
                currentWayPoint = 0;
            }            
        }
    }
}
