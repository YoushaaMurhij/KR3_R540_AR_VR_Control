# Draw a hexagon around the Target 1
from robolink import *    # RoboDK's API
from robodk import *      # Math toolbox for robots
 
# Start the RoboDK API:
RDK = Robolink()
# Get the robot (first robot found):
robot = RDK.Item('', ITEM_TYPE_ROBOT)
# Get the reference target by name:
target = RDK.Item('Target 1')
target_pose = target.Pose()
xyz_ref = target_pose.Pos()
# Move the robot to the reference point:
robot.MoveJ(target)
# Draw a hexagon around the reference target:
##for i in range(7):
##    ang = i*2*pi/6 # Angle = 0,60,120,...,360
##    R = 50        # Polygon radius
##    # Calculate the new position around the reference:
##    x = xyz_ref[0] + R*cos(ang) # new X coordinate
##    y = xyz_ref[1] + R*sin(ang) # new Y coordinate
##    z = xyz_ref[2]              # new Z coordinate    
##    target_pose.setPos([x,y,z])
##    # Move to the new target:
##    robot.MoveL(target_pose)
### Trigger a program call at the end of the movement
##robot.RunInstruction('Program_Done')
### Move back to the reference target:
##robot.MoveL(target)
##


## 
### Move back to the reference target:
##robot.MoveL(target)
##target = RDK.Item('Target 2')
##target_pose = target.Pose()
##xyz_ref = target_pose.Pos()
## 
##robot.MoveJ(target)
##num_steps=25
##R=100
##step=R/num_steps;
### Draw a circle around the reference target:
##for i in range(0, num_steps+1):
##    x=xyz_ref[0]+R-i*step;
##    y=(R**2-(x-xyz_ref[0])**2)**(0.5) + xyz_ref[1]
##    z=xyz_ref[2]
##    target_pose.setPos([x,y,z])
##    robot.MoveL(target_pose)
##for i in range(0, num_steps+1):
##    x=xyz_ref[0]-i*step;
##    y=(R**2-(x-xyz_ref[0])**2)**(0.5) + xyz_ref[1]
##    z=xyz_ref[2]
##    target_pose.setPos([x,y,z])  
##    robot.MoveL(target_pose)
##for i in range(0, num_steps+1):
##    x=xyz_ref[0]-R+i*step;
##    y=-1*((R**2-(x-xyz_ref[0])**2)**(0.5)) + xyz_ref[1]
##    z=xyz_ref[2]
##    target_pose.setPos([x,y,z])  
##    robot.MoveL(target_pose)
##for i in range(0, num_steps+1):
##    x=xyz_ref[0]+i*step;
##    y=-1*(R**2-(x-xyz_ref[0])**2)**(0.5) + xyz_ref[1]
##    z=xyz_ref[2]
##    target_pose.setPos([x,y,z])  
##    robot.MoveL(target_pose)
### Trigger a program call at the end of the movement
##robot.RunInstruction('Program_Done')
### Move back to the reference target:
##robot.MoveL(target)


 
# Move back to the reference target:
robot.MoveL(target)
target = RDK.Item('Target 2')
target_pose = target.Pose()
xyz_ref = target_pose.Pos()
 
robot.MoveJ(target)

num_steps=15
num_steps_z=10
r=100
step_z=r/num_steps_z
# Draw a sphere around the reference target:
for j in range(num_steps_z,0,-1):
    z=xyz_ref[2]-j*step_z
    R=(r**2-(z-xyz_ref[2])**2)**0.5
    step=R/num_steps
    for i in range(0, num_steps+1):
        x=xyz_ref[0]+R-i*step;
        y=(R**2-(R-i*step)**2)**(0.5) + xyz_ref[1]
        target_pose.setPos([x,y,z])
        robot.MoveL(target_pose)
    for i in range(0, num_steps+1):
        x=xyz_ref[0]-i*step;
        y=(R**2-(i*step)**2)**(0.5) + xyz_ref[1]
        target_pose.setPos([x,y,z])  
        robot.MoveL(target_pose)
    for i in range(0, num_steps+1):
        x=xyz_ref[0]-R+i*step;
        y=-1*((R**2-(-R+i*step)**2)**(0.5)) + xyz_ref[1]
        target_pose.setPos([x,y,z])  
        robot.MoveL(target_pose)
    for i in range(0, num_steps+1):
        x=xyz_ref[0]+i*step;
        y=-1*(R**2-(i*step)**2)**(0.5) + xyz_ref[1]
        target_pose.setPos([x,y,z])  
        robot.MoveL(target_pose)
for j in range(0,num_steps_z+1):
    z=xyz_ref[2]+j*step_z
    R=(r**2-(z-xyz_ref[2])**2)**0.5
    step=R/num_steps
    for i in range(0, num_steps+1):
        x=xyz_ref[0]+R-i*step;
        y=(R**2-(R-i*step)**2)**(0.5) + xyz_ref[1]
        target_pose.setPos([x,y,z])
        robot.MoveL(target_pose)
    for i in range(0, num_steps+1):
        x=xyz_ref[0]-i*step;
        y=(R**2-(i*step)**2)**(0.5) + xyz_ref[1]
        target_pose.setPos([x,y,z])  
        robot.MoveL(target_pose)
    for i in range(0, num_steps+1):
        x=xyz_ref[0]-R+i*step;
        y=-1*((R**2-(-R+i*step)**2)**(0.5)) + xyz_ref[1]
        target_pose.setPos([x,y,z])  
        robot.MoveL(target_pose)
    for i in range(0, num_steps+1):
        x=xyz_ref[0]+i*step;
        y=-1*(R**2-(i*step)**2)**(0.5) + xyz_ref[1]
        target_pose.setPos([x,y,z])  
        robot.MoveL(target_pose)
     
# Trigger a program call at the end of the movement
robot.RunInstruction('Program_Done')
 
# Move back to the reference target:
robot.MoveL(target)

