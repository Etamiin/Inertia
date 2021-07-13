using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia;
using Inertia.Realtime;

public class ExampleScript : Script
{
    private float m_timer;
    private int m_iteration;

    //this method is optional
    protected override void OnAwake(ScriptArgumentsCollection args)
    {
        //Use args collection to initialize your things
    }

    //this method execute your script in loop (while it isn't destroyed)
    protected override void OnUpdate()
    {
        //create a timer that reset each 0.1sec for 25 times

        m_timer += DeltaTime;
        if (m_timer >= .1f)
        {
            m_timer -= .1f;
            //set iteration in the console title
            Console.Title = (++m_iteration).ToString();

            if (m_iteration == 25)
            {
                //use the CustomLogger to log an "Example" pattern saying that the script ended perfectly
                this.GetLogger().LogPattern("Example", "Script ended!");

                //destroy the script
                Destroy();
            }
        }
    }
}