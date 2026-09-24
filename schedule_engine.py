BASE_TASKS = [
    {
        "task": "Site Preparation",
        "duration_days": 7
    },
    {
        "task": "Foundation",
        "duration_days": 20
    },
    {
        "task": "Wall Construction",
        "duration_days": 30
    },
    {
        "task": "Roof Construction",
        "duration_days": 15
    },
    {
        "task": "Electrical Work",
        "duration_days": 12
    },
    {
        "task": "Plumbing",
        "duration_days": 10
    },
    {
        "task": "Flooring",
        "duration_days": 12
    },
    {
        "task": "Painting",
        "duration_days": 10
    }
]


def calculate_schedule(material_quality: str = "standard"):
    material_quality = material_quality.lower()

    lead_time = 7 if material_quality == "premium" else 0

    schedule = []
    current_day = 1

    for task in BASE_TASKS:
        start_day = current_day
        end_day = current_day + task["duration_days"] - 1

        schedule.append({
            "task": task["task"],
            "start_day": start_day,
            "end_day": end_day,
            "duration_days": task["duration_days"]
        })

        current_day = end_day + 1

    total_days = current_day - 1 + lead_time

    return {
        "material_quality": material_quality,
        "material_lead_time_days": lead_time,
        "total_construction_days": total_days,
        "tasks": schedule
    }