using System.Collections;
using UnityEngine;

public class PickupAIController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float turnSpeed = 90f;

    private bool isExecuting = false;

    public void ExecuteCommands(VehicleCommand[] commands)
    {
        if (commands == null || commands.Length == 0 || isExecuting) return;
        StartCoroutine(ExecuteSequence(commands));
    }

    private IEnumerator ExecuteSequence(VehicleCommand[] commands)
    {
        isExecuting = true;

        for (int i = 0; i < commands.Length; i++)
        {
            VehicleCommand command = commands[i];

            if (command == null || string.IsNullOrEmpty(command.action))
                continue;

            if (command.action == "move")
            {
                yield return StartCoroutine(MoveRoutine(command.direction, command.distance));
            }
            else if (command.action == "turn")
            {
                yield return StartCoroutine(TurnRoutine(command.direction, command.degrees));
            }
            else if (command.action == "stop")
            {
                break;
            }
        }

        isExecuting = false;
    }

    private IEnumerator MoveRoutine(string direction, float distance)
    {
        Vector3 dir = transform.forward;

        if (direction == "backward")
            dir = -transform.forward;

        Vector3 start = transform.position;
        Vector3 target = start + dir * distance;

        while (Vector3.Distance(transform.position, target) > 0.02f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = target;
    }

    private IEnumerator TurnRoutine(string direction, float degrees)
    {
        float sign = direction == "left" ? -1f : 1f;
        float rotated = 0f;

        while (rotated < degrees)
        {
            float step = turnSpeed * Time.deltaTime;
            transform.Rotate(0f, sign * step, 0f);
            rotated += step;
            yield return null;
        }
    }
}