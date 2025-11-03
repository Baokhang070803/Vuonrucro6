# camera_follow.py
# Python implementation of Unity CameraFollow logic

class Vector3:
    def __init__(self, x=0.0, y=0.0, z=0.0):
        self.x = x
        self.y = y
        self.z = z
    
    def __add__(self, other):
        return Vector3(self.x + other.x, self.y + other.y, self.z + other.z)
    
    def __sub__(self, other):
        return Vector3(self.x - other.x, self.y - other.y, self.z - other.z)
    
    def __mul__(self, scalar):
        return Vector3(self.x * scalar, self.y * scalar, self.z * scalar)
    
    def lerp(self, target, t):
        """Linear interpolation between this vector and target"""
        return Vector3(
            self.x + (target.x - self.x) * t,
            self.y + (target.y - self.y) * t,
            self.z + (target.z - self.z) * t
        )

class CameraFollowLogic:
    def __init__(self):
        self.offset = Vector3()
        self.target_pos = Vector3()
        self.initialized = False
    
    def initialize(self, camera_pos_x, camera_pos_y, camera_pos_z, 
                   target_pos_x, target_pos_y, target_pos_z):
        """Initialize the camera follow system with camera and target positions"""
        camera_pos = Vector3(camera_pos_x, camera_pos_y, camera_pos_z)
        target_pos = Vector3(target_pos_x, target_pos_y, target_pos_z)
        
        self.offset = camera_pos - target_pos
        self.initialized = True
        
        return True
    
    def update_camera_position(self, camera_pos_x, camera_pos_y, camera_pos_z,
                              target_pos_x, target_pos_y, target_pos_z,
                              lerp_speed, delta_time):
        """Update camera position and return new position as tuple (x, y, z)"""
        if not self.initialized:
            return (camera_pos_x, camera_pos_y, camera_pos_z)
        
        # Convert input to Vector3
        current_camera_pos = Vector3(camera_pos_x, camera_pos_y, camera_pos_z)
        target_pos = Vector3(target_pos_x, target_pos_y, target_pos_z)
        
        # Calculate target position with offset
        self.target_pos = target_pos + self.offset
        
        # Lerp to target position
        lerp_factor = lerp_speed * delta_time
        new_pos = current_camera_pos.lerp(self.target_pos, lerp_factor)
        
        return (new_pos.x, new_pos.y, new_pos.z)
    
    def get_offset(self):
        """Get current offset as tuple (x, y, z)"""
        return (self.offset.x, self.offset.y, self.offset.z)
    
    def set_offset(self, x, y, z):
        """Set offset manually"""
        self.offset = Vector3(x, y, z)

# Global instance for Unity to use
camera_follow = CameraFollowLogic()

# Functions that can be called from C#
def initialize_camera_follow(camera_x, camera_y, camera_z, target_x, target_y, target_z):
    """Initialize camera follow system"""
    return camera_follow.initialize(camera_x, camera_y, camera_z, target_x, target_y, target_z)

def update_camera_follow(camera_x, camera_y, camera_z, target_x, target_y, target_z, lerp_speed, delta_time):
    """Update camera position and return new position as list [x, y, z]"""
    result = camera_follow.update_camera_position(camera_x, camera_y, camera_z, 
                                                 target_x, target_y, target_z, 
                                                 lerp_speed, delta_time)
    return list(result)

def get_camera_offset():
    """Get camera offset as list [x, y, z]"""
    result = camera_follow.get_offset()
    return list(result)

def set_camera_offset(x, y, z):
    """Set camera offset"""
    camera_follow.set_offset(x, y, z)
