#!/usr/bin/env python3
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
"""Deprecated alias — logic moved to build_knowledge_index.py (entity-ontology superset).

Kept so any pinned/older caller invoking generate-knowledge-index.py directly keeps working.
"""

from __future__ import annotations

from build_knowledge_index import main

if __name__ == "__main__":
    raise SystemExit(main())
