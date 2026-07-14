using Unity.Collections;
using UnityEngine;
using Unity.U2D.Physics;

public class Custom2DPlane : MonoBehaviour
{
    public PhysicsWorld world;
    private void Start()
    {
        PhysicsWorldDefinition worldProperties = new PhysicsWorldDefinition
        {
            transformPlane = PhysicsWorld.TransformPlane.Custom,
            transformPlaneCustom = new PhysicsWorld.TransformPlaneCustom(transform.position, transform.eulerAngles),
            gravity = Vector2.zero
        };
        world = PhysicsWorld.Create(worldProperties);

        // Create a square boundary
        PhysicsBody boundary = world.CreateBody();
        using NativeList<Vector2> extentPoints = new NativeList<Vector2>(Allocator.Temp)
        {
            new(-4f, 4f),
            new(4f, 4f),
            new(4f, -4f),
            new(-4f, -4f)
        };
        ChainGeometry boundaryWalls = new ChainGeometry(extentPoints.AsArray());
        boundary.CreateChain(boundaryWalls, PhysicsChainDefinition.defaultDefinition);

        // Create a body and set it moving
        PhysicsBodyDefinition bodyDefinition = new PhysicsBodyDefinition();
        bodyDefinition.type = PhysicsBody.BodyType.Dynamic;
        bodyDefinition.linearVelocity = new Vector2(7.3f, 5.7f);
        bodyDefinition.angularVelocity = 0f;
        PhysicsBody body = world.CreateBody(bodyDefinition);

        // Add a shape with a bouncy material
        body.transformObject = transform;
        PhysicsShapeDefinition shapeDefinition = new PhysicsShapeDefinition();
        shapeDefinition.surfaceMaterial = new PhysicsShape.SurfaceMaterial{bounciness = 1f, friction = 0f};
        body.CreateShape(new CircleGeometry { radius = 1f }, shapeDefinition);
    }

    private void OnDisable()
    {
        world.Destroy();
    }
}
