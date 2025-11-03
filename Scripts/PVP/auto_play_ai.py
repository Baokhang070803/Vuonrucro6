# auto_play_ai.py
# Python AI cho Auto Play trong PVP 3v3

import random
import time
from typing import List, Dict, Optional, Tuple

class CharacterInfo:
    """Thông tin nhân vật cho Python AI"""
    def __init__(self, name: str, team: str, position: str, 
                 current_health: float, max_health: float, 
                 is_alive: bool, skills: List[Dict]):
        self.name = name
        self.team = team
        self.position = position
        self.current_health = current_health
        self.max_health = max_health
        self.health_percentage = current_health / max_health if max_health > 0 else 0
        self.is_alive = is_alive
        self.skills = skills  # List of skill info from C#
        
    def __str__(self):
        return f"{self.name} ({self.team}) - HP: {self.current_health}/{self.max_health}"

class AutoPlayAI:
    """AI cho Auto Play trong PVP 3v3"""
    
    def __init__(self):
        self.difficulty = "medium"  # easy, medium, hard
        self.strategy_mode = "balanced"  # aggressive, defensive, balanced
        self.learning_enabled = True
        self.match_history = []
        
        # AI weights
        self.damage_priority = 0.4
        self.heal_priority = 0.3
        self.ultimate_priority = 0.3
        
    def set_difficulty(self, difficulty: str):
        """Set AI difficulty"""
        self.difficulty = difficulty
        if difficulty == "easy":
            self.damage_priority = 0.3
            self.heal_priority = 0.4
            self.ultimate_priority = 0.3
        elif difficulty == "hard":
            self.damage_priority = 0.5
            self.heal_priority = 0.2
            self.ultimate_priority = 0.3
            
    def choose_skill(self, current_character: CharacterInfo, 
                    available_skills: List[Dict], 
                    all_characters: List[CharacterInfo]) -> Tuple[int, str]:
        """
        Chọn skill tốt nhất cho character hiện tại
        
        Returns:
            Tuple[int, str]: (skill_index, reason)
        """
        if not available_skills:
            return -1, "No skills available"
            
        # Lọc skills có thể dùng
        usable_skills = [skill for skill in available_skills if skill.get('is_ready', False)]
        if not usable_skills:
            return -1, "No skills ready"
            
        # Tính điểm cho từng skill
        skill_scores = []
        for i, skill in enumerate(usable_skills):
            score, reason = self._calculate_skill_score(
                current_character, skill, all_characters
            )
            skill_scores.append((i, score, reason))
            
        # Sắp xếp theo điểm số
        skill_scores.sort(key=lambda x: x[1], reverse=True)
        
        # Chọn skill tốt nhất (có thể randomize theo difficulty)
        if self.difficulty == "easy":
            # Easy: 70% chọn skill tốt nhất, 30% random
            if random.random() < 0.7:
                best_skill = skill_scores[0]
            else:
                best_skill = random.choice(skill_scores[:3])
        elif self.difficulty == "hard":
            # Hard: Luôn chọn skill tốt nhất
            best_skill = skill_scores[0]
        else:  # medium
            # Medium: 85% chọn skill tốt nhất, 15% random
            if random.random() < 0.85:
                best_skill = skill_scores[0]
            else:
                best_skill = random.choice(skill_scores[:2])
                
        return best_skill[0], best_skill[2]
    
    def choose_target(self, current_character: CharacterInfo, 
                     skill: Dict, 
                     all_characters: List[CharacterInfo]) -> Tuple[CharacterInfo, str]:
        """
        Chọn target tốt nhất cho skill
        
        Returns:
            Tuple[CharacterInfo, str]: (target, reason)
        """
        target_type = skill.get('target_type', 'enemy')
        
        if target_type == 'self':
            return current_character, "Self-target skill"
            
        elif target_type == 'ally':
            # Chọn đồng đội có máu thấp nhất
            allies = [char for char in all_characters 
                     if char.team == current_character.team and char.is_alive]
            if not allies:
                return current_character, "No allies available"
                
            # Ưu tiên đồng đội có máu thấp nhất
            target = min(allies, key=lambda x: x.health_percentage)
            return target, f"Heal ally with lowest HP ({target.health_percentage:.1%})"
            
        else:  # enemy
            # Chọn địch có máu thấp nhất (để finish off)
            enemies = [char for char in all_characters 
                      if char.team != current_character.team and char.is_alive]
            if not enemies:
                return current_character, "No enemies available"
                
            # Strategy: Ưu tiên địch có máu thấp để finish off
            if self.strategy_mode == "aggressive":
                # Aggressive: Ưu tiên địch có máu thấp nhất
                target = min(enemies, key=lambda x: x.health_percentage)
                return target, f"Finish off enemy with lowest HP ({target.health_percentage:.1%})"
            else:
                # Balanced/Defensive: Chọn địch có damage potential cao nhất
                target = self._choose_best_enemy_target(enemies, skill)
                return target, f"Target enemy with highest threat"
    
    def _calculate_skill_score(self, character: CharacterInfo, 
                              skill: Dict, 
                              all_characters: List[CharacterInfo]) -> Tuple[float, str]:
        """Tính điểm cho skill"""
        skill_type = skill.get('skill_type', 'damage')
        damage = skill.get('damage', 0)
        heal = skill.get('heal_amount', 0)
        cooldown = skill.get('cooldown', 0)
        
        base_score = 0
        reason_parts = []
        
        # Damage skills
        if skill_type == 'damage':
            # Ưu tiên damage cao
            damage_score = damage * self.damage_priority
            base_score += damage_score
            reason_parts.append(f"Damage: {damage}")
            
            # Bonus cho ultimate skills
            if cooldown >= 4:  # Ultimate skill
                base_score += 20 * self.ultimate_priority
                reason_parts.append("Ultimate bonus")
                
        # Heal skills
        elif skill_type == 'heal':
            # Ưu tiên heal khi máu thấp
            if character.health_percentage < 0.5:
                heal_score = heal * self.heal_priority * 2  # Double priority khi máu thấp
                base_score += heal_score
                reason_parts.append(f"Heal when low HP: {heal}")
            else:
                heal_score = heal * self.heal_priority * 0.5  # Giảm priority khi máu cao
                base_score += heal_score
                reason_parts.append(f"Heal when high HP: {heal}")
                
        # Buff/Debuff skills
        elif skill_type in ['buff', 'debuff']:
            # Ưu tiên buff khi có nhiều địch
            enemy_count = len([c for c in all_characters 
                             if c.team != character.team and c.is_alive])
            if enemy_count >= 2:
                base_score += 15
                reason_parts.append("Buff against multiple enemies")
            else:
                base_score += 5
                reason_parts.append("Buff against single enemy")
        
        # Cooldown penalty
        if cooldown > 0:
            base_score -= cooldown * 2  # Penalty cho skills có cooldown cao
            
        # Random factor để tránh predictable
        random_factor = random.uniform(0.9, 1.1)
        final_score = base_score * random_factor
        
        reason = f"Score: {final_score:.1f} ({', '.join(reason_parts)})"
        return final_score, reason
    
    def _choose_best_enemy_target(self, enemies: List[CharacterInfo], 
                                 skill: Dict) -> CharacterInfo:
        """Chọn địch tốt nhất để tấn công"""
        if not enemies:
            return None
            
        # Tính threat level cho từng địch
        enemy_scores = []
        for enemy in enemies:
            score = 0
            
            # Ưu tiên địch có máu thấp (finish off)
            if enemy.health_percentage < 0.3:
                score += 50
            elif enemy.health_percentage < 0.6:
                score += 30
            else:
                score += 10
                
            # Ưu tiên địch có damage potential cao
            # (Giả sử có thể tính từ skill damage hoặc character level)
            score += enemy.health_percentage * 20  # Địch máu cao = threat cao
            
            # Random factor
            score += random.uniform(0, 10)
            
            enemy_scores.append((enemy, score))
            
        # Chọn địch có điểm cao nhất
        enemy_scores.sort(key=lambda x: x[1], reverse=True)
        return enemy_scores[0][0]
    
    def analyze_match_state(self, all_characters: List[CharacterInfo]) -> Dict:
        """Phân tích trạng thái trận đấu"""
        team_a = [c for c in all_characters if c.team == 'TeamA' and c.is_alive]
        team_b = [c for c in all_characters if c.team == 'TeamB' and c.is_alive]
        
        analysis = {
            'team_a_count': len(team_a),
            'team_b_count': len(team_b),
            'team_a_avg_hp': sum(c.health_percentage for c in team_a) / len(team_a) if team_a else 0,
            'team_b_avg_hp': sum(c.health_percentage for c in team_b) / len(team_b) if team_b else 0,
            'advantage': 'TeamA' if len(team_a) > len(team_b) else 'TeamB' if len(team_b) > len(team_a) else 'Even'
        }
        
        return analysis
    
    def get_ai_recommendation(self, current_character: CharacterInfo, 
                            available_skills: List[Dict], 
                            all_characters: List[CharacterInfo]) -> Dict:
        """Get AI recommendation cho turn hiện tại"""
        # Chọn skill
        skill_index, skill_reason = self.choose_skill(current_character, available_skills, all_characters)
        
        if skill_index == -1:
            return {
                'skill_index': -1,
                'target_index': -1,
                'reason': skill_reason,
                'confidence': 0.0
            }
            
        # Chọn target
        selected_skill = available_skills[skill_index]
        target, target_reason = self.choose_target(current_character, selected_skill, all_characters)
        
        # Tìm target index
        target_index = -1
        if target:
            for i, char in enumerate(all_characters):
                if char.name == target.name:
                    target_index = i
                    break
                    
        # Tính confidence
        confidence = self._calculate_confidence(current_character, selected_skill, target, all_characters)
        
        return {
            'skill_index': skill_index,
            'target_index': target_index,
            'skill_name': selected_skill.get('skill_name', 'Unknown'),
            'target_name': target.name if target else 'None',
            'reason': f"{skill_reason} | {target_reason}",
            'confidence': confidence
        }
    
    def _calculate_confidence(self, character: CharacterInfo, skill: Dict, 
                            target: CharacterInfo, all_characters: List[CharacterInfo]) -> float:
        """Tính confidence level cho decision"""
        confidence = 0.5  # Base confidence
        
        # Bonus cho skills có damage/heal cao
        if skill.get('damage', 0) > 30:
            confidence += 0.2
        if skill.get('heal_amount', 0) > 20:
            confidence += 0.2
            
        # Bonus cho target selection
        if target and target.health_percentage < 0.3:
            confidence += 0.2  # High confidence khi finish off
            
        # Penalty cho random decisions
        if self.difficulty == "easy" and random.random() < 0.3:
            confidence -= 0.3
            
        return max(0.0, min(1.0, confidence))

# Global instance
auto_play_ai = AutoPlayAI()

# Functions để C# gọi
def initialize_ai(difficulty: str = "medium", strategy: str = "balanced"):
    """Initialize AI với settings"""
    auto_play_ai.set_difficulty(difficulty)
    auto_play_ai.strategy_mode = strategy
    return {"success": True, "message": f"AI initialized: {difficulty} difficulty, {strategy} strategy"}

def get_ai_recommendation(character_name: str, character_team: str, character_position: str,
                         current_health: float, max_health: float, is_alive: bool,
                         skills_data: list, all_characters_data: list):
    """Get AI recommendation cho character"""
    try:
        # Tạo CharacterInfo cho current character
        current_char = CharacterInfo(
            name=character_name,
            team=character_team,
            position=character_position,
            current_health=current_health,
            max_health=max_health,
            is_alive=is_alive,
            skills=skills_data
        )
        
        # Tạo CharacterInfo list cho all characters
        all_chars = []
        for char_data in all_characters_data:
            char = CharacterInfo(
                name=char_data['name'],
                team=char_data['team'],
                position=char_data['position'],
                current_health=char_data['current_health'],
                max_health=char_data['max_health'],
                is_alive=char_data['is_alive'],
                skills=char_data.get('skills', [])
            )
            all_chars.append(char)
            
        # Get recommendation
        recommendation = auto_play_ai.get_ai_recommendation(current_char, skills_data, all_chars)
        
        return {
            "success": True,
            "recommendation": recommendation
        }
        
    except Exception as e:
        return {
            "success": False,
            "message": f"AI error: {str(e)}"
        }

def analyze_match_state(all_characters_data: list):
    """Analyze current match state"""
    try:
        all_chars = []
        for char_data in all_characters_data:
            char = CharacterInfo(
                name=char_data['name'],
                team=char_data['team'],
                position=char_data['position'],
                current_health=char_data['current_health'],
                max_health=char_data['max_health'],
                is_alive=char_data['is_alive'],
                skills=char_data.get('skills', [])
            )
            all_chars.append(char)
            
        analysis = auto_play_ai.analyze_match_state(all_chars)
        return {
            "success": True,
            "analysis": analysis
        }
        
    except Exception as e:
        return {
            "success": False,
            "message": f"Analysis error: {str(e)}"
        }
