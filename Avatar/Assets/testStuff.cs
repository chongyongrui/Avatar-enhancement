using Npgsql;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UnityEngine;

public class testStuff : MonoBehaviour
{


    public Transform RshoulderTransform;
    public Transform RelbowTransform; 
    public Transform RhipTransform;
    public Transform RkneeTransform;
    public Transform LshoulderTransform;
    public Transform LelbowTransform; 
    public Transform LhipTransform;
    public Transform LkneeTransform;

    public List<PoseData> danceMoves = new List<PoseData>();
    public bool startDancing = false;
    public int danceFrame = 0;
    public int frameCount = 0;

    public float Rshoulder, Relbow, Rhip, Rknee;


    public struct PoseData
    {
        public float Rhip_x, Rhip_y, Rshoulder_x, Rshoulder_y, Relbow_x, Relbow_y, Rwrist_x, Rwrist_y,
        Rknee_x, Rknee_y, Rankle_x, Rankle_y, Lhip_x, Lhip_y, Lshoulder_x, Lshoulder_y,
        Lelbow_x, Lelbow_y, Lwrist_x, Lwrist_y, Lknee_x, Lknee_y, Lankle_x, Lankle_y,
        RshoulderBend, RelbowBend, RhipBend, Rkneebend,
        LshoulderBend, LelbowBend, LhipBend, Lkneebend;
        public int frames;
    }


    public void Start()
    {
        GetDanceMoves();
    }


    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("PRESSED M");
            GetDanceMoves();
            startDancing =true;
        }

        
        if (startDancing)
        {
            frameCount++;

            if (danceFrame < danceMoves.Count && frameCount % 2 == 0)
            {
                PoseData poseData = danceMoves[danceFrame];
                moveBody(poseData);
                danceFrame++;
            }
            else if (danceFrame < danceMoves.Count && frameCount % 2 != 0)
            {

            }
            else
            {
                startDancing =false;
                danceFrame = 0;
                frameCount = 0;
            }
        }
    }
    public void GetDanceMoves()
    {

        danceMoves.Clear();


        try
        {


            using (NpgsqlConnection connection = new NpgsqlConnection("Server=localhost;Port=5433;User Id=sysadmin;Password=D5taCard;Database=postgres;"))
            {

                connection.Open();


                using (var command = connection.CreateCommand())
                {

                    command.CommandText = "SELECT * FROM pose_data";

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            PoseData pose = new PoseData();

                            pose.Rhip_x = float.Parse(reader["Rhip_x"].ToString());
                            pose.Rhip_y = float.Parse(reader["Rhip_y"].ToString());
                            pose.Rshoulder_x = float.Parse(reader["Rshoulder_x"].ToString());
                            pose.Rshoulder_y = float.Parse(reader["Rshoulder_y"].ToString());
                            pose.Relbow_x = float.Parse(reader["Relbow_x"].ToString());
                            pose.Relbow_y = float.Parse(reader["Relbow_y"].ToString());
                            pose.Rwrist_x = float.Parse(reader["Rwrist_x"].ToString());
                            pose.Rwrist_y = float.Parse(reader["Rwrist_y"].ToString());
                            pose.Rknee_x = float.Parse(reader["Rknee_x"].ToString());
                            pose.Rknee_y = float.Parse(reader["Rknee_y"].ToString());
                            pose.Rankle_x = float.Parse(reader["Rankle_x"].ToString());
                            pose.Rankle_y = float.Parse(reader["Rankle_y"].ToString());

                            pose.Lhip_x = float.Parse(reader["Lhip_x"].ToString());
                            pose.Lhip_y = float.Parse(reader["Lhip_y"].ToString());
                            pose.Lshoulder_x = float.Parse(reader["Lshoulder_x"].ToString());
                            pose.Lshoulder_y = float.Parse(reader["Lshoulder_y"].ToString());
                            pose.Lelbow_x = float.Parse(reader["Lelbow_x"].ToString());
                            pose.Lelbow_y = float.Parse(reader["Lelbow_y"].ToString());
                            pose.Lwrist_x = float.Parse(reader["Lwrist_x"].ToString());
                            pose.Lwrist_y = float.Parse(reader["Lwrist_y"].ToString());
                            pose.Lknee_x = float.Parse(reader["Lknee_x"].ToString());
                            pose.Lknee_y = float.Parse(reader["Lknee_y"].ToString());
                            pose.Lankle_x = float.Parse(reader["Lankle_x"].ToString());
                            pose.Lankle_y = float.Parse(reader["Lankle_y"].ToString());

                            pose.RshoulderBend = float.Parse(reader["RshoulderBend"].ToString());
                            pose.RelbowBend = float.Parse(reader["RelbowBend"].ToString());
                            pose.RhipBend = float.Parse(reader["RhipBend"].ToString());
                            pose.Rkneebend = float.Parse(reader["Rkneebend"].ToString());

                            pose.LshoulderBend = float.Parse(reader["LshoulderBend"].ToString());
                            pose.LelbowBend = float.Parse(reader["LelbowBend"].ToString());
                            pose.LhipBend = float.Parse(reader["LhipBend"].ToString());
                            pose.Lkneebend = float.Parse(reader["Lkneebend"].ToString());

                            pose.frames = Int32.Parse(reader["scenenumber"].ToString());


                            danceMoves.Add(pose);
                            


                        }
                        reader.Close();
                    }

                    danceMoves.Sort((pose1, pose2) => pose1.frames.CompareTo(pose2.frames));
                }
                connection.Close();


            }
        }
        catch (Exception e)
        {
            Debug.Log("(SQL Server) Error updating player location " + e);

        }

    }


    public void moveBody(PoseData data)
    {


        // Calculate the position of the shoulder and elbow in world space
        /*Vector3 RwristPosition = new Vector3(data.Rwrist_x, data.Rwrist_y, 0);
        Vector3 RelbowPosition = new Vector3(data.Relbow_x, data.Relbow_y, 0);
        Vector3 RhipPosition = new Vector3(data.Rhip_x, data.Rhip_y, 0);
        Vector3 RshoulderPosition = new Vector3(data.Rshoulder_x, data.Rshoulder_y, 0);
        Vector3 RanklePosition = new Vector3(data.Rankle_x, data.Rankle_y, 0);
        Vector3 RkneePosition = new Vector3(data.Rknee_x, data.Rknee_y, 0);
        Vector3 LwristPosition = new Vector3(data.Lwrist_x, data.Lwrist_y, 0);
        Vector3 LelbowPosition = new Vector3(data.Lelbow_x, data.Lelbow_y, 0);
        Vector3 LhipPosition = new Vector3(data.Lhip_x, data.Lhip_y, 0);
        Vector3 LshoulderPosition = new Vector3(data.Lshoulder_x, data.Lshoulder_y, 0);
        Vector3 LanklePosition = new Vector3(data.Lankle_x, data.Lankle_y, 0);
        Vector3 LkneePosition = new Vector3(data.Lknee_x, data.Lknee_y, 0);

        // Calculate the direction from shoulder to elbow
        Vector3 RshoulderDirection = RelbowPosition - RshoulderPosition;
        Vector3 RelbowDirection = RelbowPosition - RshoulderPosition;
        Vector3 RhipDirection = RhipPosition - RshoulderPosition;
        Vector3 RkneeDirection = RkneePosition - RhipPosition;
        // Calculate the direction from shoulder to elbow
        Vector3 LshoulderDirection = LelbowPosition - LshoulderPosition;
        Vector3 LelbowDirection = LelbowPosition - LshoulderPosition;
        Vector3 LhipDirection = LhipPosition - LshoulderPosition;
        Vector3 LkneeDirection = LkneePosition - LhipPosition;*/

        // Rotate the shoulder to face the elbow direction

       /* LshoulderTransform.rotation = Quaternion.LookRotation(Vector3.right, RshoulderDirection);
        LelbowTransform.rotation = Quaternion.LookRotation(Vector3.right, RelbowDirection);
        LhipTransform.rotation = Quaternion.LookRotation(Vector3.forward, -RhipDirection);
        LkneeTransform.rotation = Quaternion.LookRotation(Vector3.forward, -RkneeDirection);
        RshoulderTransform.rotation = Quaternion.LookRotation(Vector3.right, LshoulderDirection);
        RelbowTransform.rotation = Quaternion.LookRotation(Vector3.right, LelbowDirection);
        RhipTransform.rotation = Quaternion.LookRotation(Vector3.forward, -LhipDirection);
        RkneeTransform.rotation = Quaternion.LookRotation(Vector3.forward, -LkneeDirection);*/

        LshoulderTransform.rotation = Quaternion.Euler(0,0,180-data.LshoulderBend);
        //LelbowTransform.rotation = Quaternion.Euler(0,0, (-data.LelbowBend));
        LhipTransform.rotation = Quaternion.Euler(0, 0, data.LhipBend);
        LkneeTransform.rotation = Quaternion.Euler(0, 0, data.LhipBend-data.Lkneebend+180);

        RshoulderTransform.rotation = Quaternion.Euler(0, 0, 180+data.RshoulderBend);
       // RelbowTransform.rotation = Quaternion.Euler(0, 0, data.RelbowBend);
        RhipTransform.rotation = Quaternion.Euler(0, 0, data.RhipBend);
        RkneeTransform.rotation = Quaternion.Euler(0, 0, data.RhipBend - data.Rkneebend+180);
        Rshoulder = data.RshoulderBend;
        Rhip = data.RhipBend;
        Rknee = data.Rkneebend;
        Relbow = data.RelbowBend;

        

    }
}


