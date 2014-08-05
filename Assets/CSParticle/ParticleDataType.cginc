struct WorldData
{
	float timestep;
	float particle_size;
	float particle_lifetime;
	float wall_stiffness;
	float pressure_stiffness;
	float decelerate;
	float gravity;
	int num_max_particles;
	int num_particles;
	int num_sphere_colliders;
	int num_capsule_colliders;
	int num_box_colliders;
	int num_forces;
	float3 world_center;
	float3 world_extents;
	int3 world_div;
	int3 world_div_bits;
	uint3 world_div_shift;
	float3 world_cellsize;
	float3 rcp_world_cellsize;
	float2 rt_size;
	float4x4 view_proj;
	float rcp_particle_size2;
	float3 coord_scaler;
};

struct Cell
{
	uint begin;
	uint end;
};

struct Particle
{
	float3 position;
	float3 velocity;
	float speed;
	float lifetime;
	int hash;
	int hit_objid;
};

struct ParticleIData
{
	float3 accel;
};


struct Sphere
{
	float3 center;
	float radius;
};

struct Capsule
{
	float3 pos1;
	float3 pos2;
	float radius;
};

struct Plane
{
	float3 normal;
	float distance;
};

struct Box
{
	float3 center;
	Plane planes[6];
};

struct AABB
{
	float3 center;
	float3 extents;
};

struct ColliderInfo
{
	int owner_objid;
	AABB aabb;
};


struct SphereCollider
{
	ColliderInfo info;
	Sphere shape;
};

struct CapsuleCollider
{
	ColliderInfo info;
	Capsule shape;
};

struct BoxCollider
{
	ColliderInfo info;
	Box shape;
};


struct ForceInfo
{
	int shape_type; // 0: affect all, 1: sphere, 2: capsule, 3: box
	int dir_type; // 0: directional, 1: radial
	float strength;
	float3 direction;
	float3 center;
};

struct Force
{
	ForceInfo info;
	Sphere sphere;
	Capsule capsule;
	Box box;
};
