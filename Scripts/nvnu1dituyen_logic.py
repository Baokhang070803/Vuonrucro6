# nvnu1dituyen_logic.py
# Python implementation of Unity Player Movement Logic

import math

class Vector2:
    def __init__(self, x=0.0, y=0.0):
        self.x = x
        self.y = y
    
    def __add__(self, other):
        return Vector2(self.x + other.x, self.y + other.y)
    
    def __sub__(self, other):
        return Vector2(self.x - other.x, self.y - other.y)
    
    def __mul__(self, scalar):
        return Vector2(self.x * scalar, self.y * scalar)
    
    @property
    def sqrMagnitude(self):
        return self.x * self.x + self.y * self.y
    
    @property
    def magnitude(self):
        return math.sqrt(self.sqrMagnitude)
    
    @property
    def normalized(self):
        mag = self.magnitude
        if mag > 0.0001:
            return Vector2(self.x / mag, self.y / mag)
        return Vector2(0, 0)
    
    @staticmethod
    def zero():
        return Vector2(0, 0)

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
    
    @property
    def sqrMagnitude(self):
        return self.x * self.x + self.y * self.y + self.z * self.z
    
    @property
    def magnitude(self):
        return math.sqrt(self.sqrMagnitude)
    
    @property
    def normalized(self):
        mag = self.magnitude
        if mag > 0.0001:
            return Vector3(self.x / mag, self.y / mag, self.z / mag)
        return Vector3(0, 0, 0)
    
    @staticmethod
    def distance(a, b):
        diff = a - b
        return diff.magnitude
    
    @staticmethod
    def zero():
        return Vector3(0, 0, 0)

class RaycastHit2D:
    def __init__(self):
        self.collider = None
        self.distance = 0.0
        self.point = Vector2()
        self.normal = Vector2()

class PlayerMovementLogic:
    def __init__(self):
        # Movement variables
        self.move_speed = 5.0
        self.move_input = Vector2.zero()
        self.is_sprinting = False
        self.movement = Vector2.zero()
        self.mouse_target = None
        
        # Village Entry Quest variables
        self.village_entry_position = Vector3(0, 0, 0)
        self.entry_distance = 2.0
        self.has_entered_village = False
        
        # Animation states
        self.horizontal = 0.0
        self.vertical = 0.0
        self.speed = 0.0
        
        # Physics state
        self.current_position = Vector3.zero()
        self.should_move = True
        
    def initialize(self, move_speed, village_entry_x, village_entry_y, village_entry_z, entry_distance):
        """Initialize the player movement system"""
        self.move_speed = move_speed
        self.village_entry_position = Vector3(village_entry_x, village_entry_y, village_entry_z)
        self.entry_distance = entry_distance
        self.has_entered_village = False
        return True
    
    def process_input(self, a_pressed, d_pressed, w_pressed, s_pressed, shift_pressed):
        """Process keyboard input"""
        self.move_input = Vector2.zero()
        self.is_sprinting = False
        
        if a_pressed:
            self.move_input.x = -1.0
        if d_pressed:
            self.move_input.x = 1.0
        if w_pressed:
            self.move_input.y = 1.0
        if s_pressed:
            self.move_input.y = -1.0
        if shift_pressed:
            self.is_sprinting = True
            
        self.movement = self.move_input
    
    def process_mouse_click(self, mouse_world_x, mouse_world_y, mouse_world_z):
        """Process mouse click for movement target"""
        self.mouse_target = Vector3(mouse_world_x, mouse_world_y, mouse_world_z)
    
    def update_animation_states(self, current_pos_x, current_pos_y, current_pos_z):
        """Update animation parameters"""
        self.current_position = Vector3(current_pos_x, current_pos_y, current_pos_z)
        
        if self.mouse_target is not None:
            direction = (self.mouse_target - self.current_position).normalized
            self.horizontal = direction.x
            self.vertical = direction.y
            self.speed = direction.sqrMagnitude
        else:
            self.horizontal = self.movement.x
            self.vertical = self.movement.y
            self.speed = self.movement.sqrMagnitude
    
    def calculate_movement_direction(self, current_pos_x, current_pos_y, current_pos_z, delta_time):
        """Calculate movement direction and speed"""
        self.current_position = Vector3(current_pos_x, current_pos_y, current_pos_z)
        
        current_speed = self.move_speed
        if self.is_sprinting:
            current_speed *= 2.0
            
        move_dir = Vector2.zero()
        
        if self.mouse_target is not None:
            direction = (self.mouse_target - self.current_position).normalized
            move_dir = Vector2(direction.x, direction.y)
            
            # Check if reached mouse target
            distance_to_target = Vector3.distance(self.current_position, self.mouse_target)
            if distance_to_target < 0.1:
                self.mouse_target = None
        else:
            move_dir = self.movement.normalized
            
        return {
            'move_dir_x': move_dir.x,
            'move_dir_y': move_dir.y,
            'current_speed': current_speed,
            'distance': current_speed * delta_time
        }
    
    def should_stop_movement_on_collision(self):
        """Handle collision response"""
        # Dừng di chuyển và huỷ mục tiêu chuột để không cố gắng đẩy NPC
        self.mouse_target = None
        return True
    
    def check_village_entry(self, current_pos_x, current_pos_y, current_pos_z, village_target_x=None, village_target_y=None, village_target_z=None):
        """Check if player has entered village area"""
        if self.has_entered_village:
            return False
            
        self.current_position = Vector3(current_pos_x, current_pos_y, current_pos_z)
        
        # Ưu tiên sử dụng villageEntryTarget nếu có, nếu không thì dùng villageEntryPosition
        if village_target_x is not None and village_target_y is not None and village_target_z is not None:
            target_position = Vector3(village_target_x, village_target_y, village_target_z)
        else:
            target_position = self.village_entry_position
            
        distance = Vector3.distance(self.current_position, target_position)
        if distance <= self.entry_distance:
            self.has_entered_village = True
            
            # Dừng di chuyển ngay lập tức
            self.stop_movement()
            
            return True  # Signal to complete quest
            
        return False
    
    def stop_movement(self):
        """Stop all movement"""
        # Dừng tất cả di chuyển
        self.mouse_target = None
        self.move_input = Vector2.zero()
        self.movement = Vector2.zero()
        
        # Đặt animation về idle
        self.horizontal = 0.0
        self.vertical = 0.0
        self.speed = 0.0
    
    def handle_dialogue_state(self, is_dialogue_open):
        """Handle movement when dialogue is open"""
        if is_dialogue_open:
            self.mouse_target = None
            self.horizontal = 0.0
            self.vertical = 0.0
            self.speed = 0.0
            return True  # Should return early from update
        return False
    
    def get_animation_parameters(self):
        """Get current animation parameters"""
        return {
            'horizontal': self.horizontal,
            'vertical': self.vertical,
            'speed': self.speed
        }

# Global instance for Unity to use
player_movement = PlayerMovementLogic()

# Functions that can be called from C#
def initialize_player_movement(move_speed, village_entry_x, village_entry_y, village_entry_z, entry_distance):
    """Initialize player movement system"""
    return player_movement.initialize(move_speed, village_entry_x, village_entry_y, village_entry_z, entry_distance)

def process_player_input(a_pressed, d_pressed, w_pressed, s_pressed, shift_pressed):
    """Process keyboard input"""
    player_movement.process_input(a_pressed != 0, d_pressed != 0, w_pressed != 0, s_pressed != 0, shift_pressed != 0)

def process_mouse_click_target(mouse_world_x, mouse_world_y, mouse_world_z):
    """Process mouse click for movement target"""
    player_movement.process_mouse_click(mouse_world_x, mouse_world_y, mouse_world_z)

def update_player_animation_states(current_pos_x, current_pos_y, current_pos_z):
    """Update animation parameters"""
    player_movement.update_animation_states(current_pos_x, current_pos_y, current_pos_z)

def calculate_player_movement_direction(current_pos_x, current_pos_y, current_pos_z, delta_time):
    """Calculate movement direction and speed, returns dict"""
    result = player_movement.calculate_movement_direction(current_pos_x, current_pos_y, current_pos_z, delta_time)
    return [result['move_dir_x'], result['move_dir_y'], result['current_speed'], result['distance']]

def handle_player_collision():
    """Handle collision response"""
    return player_movement.should_stop_movement_on_collision()

def check_player_village_entry(current_pos_x, current_pos_y, current_pos_z, village_target_x=None, village_target_y=None, village_target_z=None):
    """Check if player has entered village area"""
    if village_target_x is not None:
        return player_movement.check_village_entry(current_pos_x, current_pos_y, current_pos_z, village_target_x, village_target_y, village_target_z)
    else:
        return player_movement.check_village_entry(current_pos_x, current_pos_y, current_pos_z)

def stop_player_movement():
    """Stop all movement"""
    player_movement.stop_movement()

def handle_player_dialogue_state(is_dialogue_open):
    """Handle movement when dialogue is open"""
    return player_movement.handle_dialogue_state(is_dialogue_open != 0)

def get_player_animation_parameters():
    """Get current animation parameters as list [horizontal, vertical, speed]"""
    params = player_movement.get_animation_parameters()
    return [params['horizontal'], params['vertical'], params['speed']]